using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling;

/*
This class is attempt #2 to sparsely index the line numbers in a file.

It aims to:

   1. Enable quick access to any specified line in a file
   2. Index according to a compression rate i.e. no need to index every line [less memory]
   3. Dynamically resize head / middle / tail index containers as file grows [Future refactor]
   4. Ensure Tail is initally small and can be returned to the consumer very quickly

    An alternative solution to what I got now is to do a triple index (as per point 3).
    In doing so it would make the rx easier and get rid of the need for the observable list
*/
public class Indexer : IDisposable
{
    private readonly IDisposable _cleanUp;
    private readonly ISourceList<Index> _indicies = new SourceList<Index>();

    public Encoding Encoding { get; private set; }
    public FileInfo Info { get; private set; }

    public IObservable<IndexCollection> Result { get; }

    public Indexer([NotNull] IObservable<FileSegmentCollection> fileSegments,
        int compression = 10,
        int tailSize = 1000000,
        int sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing= 250000000,
        Encoding encoding = null,
        IScheduler scheduler = null)
    {
        if (fileSegments == null) throw new ArgumentNullException(nameof(fileSegments));

        //TODO: When File segment has got smaller => roll-over [do something about it]

        scheduler = scheduler ?? Scheduler.Default;

        var shared = fileSegments.Replay(1).RefCount();
           
        //1. Get information from segment info
        var infoSubscriber = shared.Select(segments => segments.Info)
            .Subscribe(info =>
            {
                Info = info;

                if (Encoding == null || info.Name != Info.Name)
                    Encoding = encoding ?? info.GetEncoding();
            });


        //2. create  a resulting index object from the collection of index fragments
        Result = _indicies
            .Connect()
            .Sort(SortExpressionComparer<Index>.Ascending(si => si.Start))
            .ToCollection()
            .Scan((IndexCollection)null, (previous, notification) => new IndexCollection(notification, previous, Info, Encoding))
            .Replay(1).RefCount();


        //3. Scan the tail so results can be returned quickly
        var tailScanner= shared.Select(segments => segments.Tail)
            .DistinctUntilChanged()
            .Scan((Index)null, (previous, current) =>
            {
                if (previous == null)
                {
                    var initial = Scan(current.Start, -1, 1);
                    return initial ?? new Index(0, 0,0,0,IndexType.Tail);
                }
                var latest=Scan(previous.End , -1, 1);
                return latest == null ? null : new Index(latest,previous);
            })
            .Where(tail=>tail!=null)
            .Replay(1).RefCount();

        //4. estimate =
        var tailSubscriber = tailScanner.Subscribe(tail =>
        {
            _indicies.Edit(innerList =>
            {
                var existing = innerList.FirstOrDefault(si => si.Type == IndexType.Tail);
                if (existing != null) innerList.Remove(existing);
                innerList.Add(tail);
            });
        });
            
        //Scan the remainer of the file
        var headSubscriber = tailScanner.FirstAsync()
            .Subscribe(tail =>
            {
                if (tail.Start == 0) return;

                //Need iterate one at a time through the 
                var estimateLines = EstimateNumberOfLines(tail, Info);
                var estimate = new Index(0, tail.Start, compression, estimateLines, IndexType.Page);
                _indicies.Add(estimate);

                //keep it as an estimate for files over 250 meg [for now]
                if (tail.Start > sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing) return;

                //todo: index first and last segment for large sized file

                scheduler.Schedule(() =>
                {
                    var actual = Scan(0, tail.Start, compression);
                    _indicies.Edit(innerList =>
                    {
                        innerList.Remove(estimate);
                        innerList.Add(actual);
                    });
                });
            });
            
        _cleanUp = new CompositeDisposable(infoSubscriber, _indicies, tailSubscriber, tailSubscriber, headSubscriber);
    }

    private int EstimateNumberOfLines(Index tail, FileInfo info)
    {
        //Calculate estimate line count
        var averageLineLength = tail.Size / tail.LineCount;
        var estimatedLines = (info.Length - tail.Size) / averageLineLength;
        return (int)estimatedLines;
    }

    private Index Scan( long start, long end, int compression)
    {
        int count = 0;
        long lastPosition = 0;
        using (var stream = File.Open(Info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
        {
            long[] lines;
            using (var reader = new StreamReaderExtended(stream, Encoding, false))
            {
                var currentPosition = reader.AbsolutePosition();
                if (currentPosition!= start)
                    stream.Seek(start, SeekOrigin.Begin);
                if (reader.EndOfStream) return null;

                lines = ScanLines(reader,compression, i => i, (line, position) =>
                {
                    var shouldBreak = end != -1 && lastPosition >= end;
                    if (!shouldBreak)
                    {
                        //do not count the last line as this will take us one line over
                        lastPosition = position;
                        count++;
                    }
                    return shouldBreak;

                }).ToArray();
            }

            if (end != -1 && lastPosition> end)
            {
                count--;
                lastPosition = end;
                lines = lines.Take(lines.Length - 1).ToArray();
            }

            return new Index(start, lastPosition, lines, compression, count, end == -1 ? IndexType.Tail : IndexType.Page);
        }
    }

    private  static IEnumerable<T> ScanLines<T>( StreamReaderExtended source,
        int compression,
        Func<long, T> selector,
        Func<string, long, bool> shouldBreak)
    {

        int i = 0;
        if (source.EndOfStream) yield break;

        string line;
        while ((line = source.ReadLine()) != null)
        {
            i++;
            var position = source.AbsolutePosition();

            if (shouldBreak(line, position))
                yield break;

            if (i == compression)
            {
                yield return selector(position);
                i = 0;
            }
        }
    }

    public void Dispose()
    {
        _cleanUp.Dispose();
    }
}