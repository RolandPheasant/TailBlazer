using System;
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
    public class SparseIndexer2 : IDisposable
    {
        private readonly IDisposable _cleanUp;
        private readonly ISourceList<SparseIndex> _indicies = new SourceList<SparseIndex>();
        //   private readonly ISourceCache<FileSegment,int> _segments = new SourceCache<FileSegment, int>(segment=> segment.Index);

        public Encoding Encoding { get; private set; }
        public FileInfo Info { get; private set; }
        public int Compression { get; }
        public int TailSize { get; }
        public IObservable<SparseIndexCollection> Result { get; }

        public SparseIndexer2([NotNull] IObservable<FileSegments> fileSegments,
            int compression = 10,
            int tailSize = 1000000,
            Encoding encoding = null,
            IScheduler scheduler = null)
        {
            if (fileSegments == null) throw new ArgumentNullException(nameof(fileSegments));

            scheduler = scheduler ?? Scheduler.Default;
            Compression = compression;
            TailSize = tailSize;

            //1. create  a resulting index object from the collection of index fragments
            Result = _indicies
                .Connect()
                .Sort(SortExpressionComparer<SparseIndex>.Ascending(si => si.Start))
                .ToCollection()
                .Scan((SparseIndexCollection)null, (previous, notification) => new SparseIndexCollection(notification, previous, Encoding))
                .Replay(1).RefCount();

            var shared = fileSegments.Replay(1).RefCount();

            //2. Get information from segment info
            var infoSubscriber = shared.Select(segments => segments.Info)
                .Take(1)
                .Subscribe(info =>
                {
                    Info = info;
                    Encoding = encoding ?? info.GetEncoding();
                });
            
            //var changedSegments = _segments.Connect()
            //                .IgnoreUpdateWhen((current, previous) => previous == current)
            //                .Transform()
            //                .AsObservableCache();

            //  var loader = shared.Subscribe(segments=>_segments.AddOrUpdate(segments.Segments));


            //3. Scan the tail so results can be returned quickly
            var tailScanner= shared.Select(segments => segments.Tail).DistinctUntilChanged()
              //  .ObserveOn(scheduler)
                .Scan((SparseIndex)null, (previous, current) =>
                {
                   
                    if (previous == null)
                    {
                        return Scan(current.Start, -1, 1);
                    }
                    var latest=Scan(previous.End , -1, 1);
                    return latest == null ? null : new SparseIndex(latest,previous);
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


            //Scan the remainer -> TODO: Scan segments, or just forget about line numbers
            var headSubscriber = tailScanner.FirstAsync()
                .Subscribe(tail =>
                {
                    if (tail.Start == 0) return;

                    //Need iterate one at a time through the 
                    var estimateLines = EstimateNumberOfLines(tail, Info);
                    var estimate = new SparseIndex(0, tail.Start, compression, estimateLines, IndexType.Page);
                    _indicies.Add(estimate);

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


            _cleanUp = new CompositeDisposable(infoSubscriber,_indicies, tailSubscriber, tailSubscriber, headSubscriber);
        }

        private int EstimateNumberOfLines(SparseIndex tail, FileInfo info)
        {
            //Calculate estimate line count
            var averageLineLength = tail.Size / tail.LineCount;
            var estimatedLines = (info.Length - tail.Size) / averageLineLength;
            return (int)estimatedLines;
        }

        private SparseIndex Scan( long start, long end, int compression)
        {
            int count = 0;
            long lastPosition = 0;
            using (var stream = File.Open(Info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                long[] lines;
                using (var reader = new StreamReaderExtended(stream, Encoding, false))
                {
                    stream.Seek(start, SeekOrigin.Begin);
                    if (reader.EndOfStream) return null;

                    lines = reader.ScanLines(compression, i => i, (line, position) =>
                    {
                        lastPosition = position;
                        count++;
                        return end != -1 && lastPosition >= end;

                    }).ToArray();
                }
                return new SparseIndex(start, lastPosition, lines, compression, count, end == -1 ? IndexType.Tail : IndexType.Page);
            }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}