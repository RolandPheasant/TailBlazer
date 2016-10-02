using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public class FileIndexer : IDisposable
    {
        private readonly IDisposable _cleanUp;
        private readonly ISourceList<Index> _indicies = new SourceList<Index>();

        public IObservable<IndexCollection> Result { get; }

        public FileIndexer([NotNull] IObservable<FileSegmentsWithTail> fileSegments,
            int compression = 10,
            int sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing= 250000000,
            IScheduler scheduler = null)
        {
            if (fileSegments == null) throw new ArgumentNullException(nameof(fileSegments));

            scheduler = scheduler ?? Scheduler.Default;

            var shared = fileSegments.Publish();

            //0.5 ensure we have the latest file info and encoding
            Encoding encoding = null;
            FileInfo fileInfo = null;

            //TODO: This is shit. We need a better way of passing around the Encoding / File meta data
            var infoSubscriber = shared
                .Where(fs => fs.Segments.Encoding != null)
                .Take(1)
                .Subscribe(fswt=>
                    {
                        encoding = fswt.Segments.Encoding;
                        fileInfo = fswt.Segments.Info;
                    });

            //1. create  a resulting index object from the collection of index fragments
            Result = _indicies
                .Connect()
                .Sort(SortExpressionComparer<Index>.Ascending(si => si.Start))
                .ToCollection()
                .Scan((IndexCollection)null, (previous, notification) => new IndexCollection(notification, previous, fileInfo, encoding))
                .Replay(1).RefCount();


            //2. continual indexing of the tail + replace tail index whenether there are new scan results
            var tailScanner = shared
                .Scan((Index) null, (previous, current) =>
                {
                    //index the tail
                    var tail = current.TailInfo;
                    var indicies = tail.Lines.Select(l => l.LineInfo.Start).ToArray();
                    var index = new Index(tail.Start, tail.End, indicies, 1, tail.Count, IndexType.Tail);
                    return previous == null ? index : new Index(index, previous);
                }).Subscribe(tail =>
                {
                    //write tail  
                    _indicies.Edit(innerList =>
                    {
                        var existing = innerList.FirstOrDefault(si => si.Type == IndexType.Tail);
                        if (existing != null) innerList.Remove(existing);
                        innerList.Add(tail);
                    });
                });

           
            //3. Index the remainer of the file, only after the head has been indexed
            var headSubscriber = shared
                .FirstAsync()
                .Subscribe(segments =>
                {
                    var tail = segments.TailInfo;
                    var segment = segments.Segments;

                    if (tail.Start == 0) return;

                    //Add an estimated number of lines
                    var estimateLines = EstimateNumberOfLines(segments);
                    var estimate = new Index(0, tail.Start, compression, estimateLines, IndexType.Page);
                    _indicies.Add(estimate);

                    //keep it as an estimate for files over 250 meg [for now]
                    if (tail.Start > sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing) return;

                    //todo: index first and last segment for large sized file
                    //Index the remainder of the file
                    scheduler.Schedule(() =>
                    {
                        var actual = Scan(segment.Info.FullName, segment.Encoding, 0, tail.Start, compression);
                        _indicies.Edit(innerList =>
                        {
                            innerList.Remove(estimate);
                            innerList.Add(actual);
                        });
                    });
                });
            
            _cleanUp = new CompositeDisposable(shared.Connect(), tailScanner, _indicies,  headSubscriber, infoSubscriber);
        }
        
        private int EstimateNumberOfLines(FileSegmentsWithTail fileSegmentsWithTail)
        {
            var tail = fileSegmentsWithTail.TailInfo;
            var segment = fileSegmentsWithTail.Segments;

            //TODO: Need to account for line delimiter
            //Calculate estimate line count
            var averageLineLength = tail.Size / tail.Count;
            var estimatedLines = (segment.TailStartsAt) / averageLineLength;
            return (int)estimatedLines;
        }

        private Index Scan(string fileName, Encoding encoding, long start, long end, int compression)
        {
            int count = 0;
            long lastPosition = 0;
            using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                long[] lines;
                using (var reader = new StreamReaderExtended(stream, encoding, false))
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
}