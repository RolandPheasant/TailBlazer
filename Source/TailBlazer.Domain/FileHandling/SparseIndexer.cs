using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    /*
        This class is attempt #1 to sparsely index the line numbers in a file.

        It aims to:

           1. Enable quick access to any specified line in a file
           2. Index according to a compression rate i.e. no need to index every line [less memory]
           3. Dynamically resize head / middle / tail index containers as file grows [Future refactor]
           4. Ensure Tail is initally small and can be returned to the consumer very quickly

            An alternative solution to what I got now is to do a triple index (as per point 3).
            In doing so it would make the rx easier and get rid of the need for the observable list
      */
    public class SparseIndexer: IDisposable
    {
        private readonly IDisposable _cleanUp;
        private int _endOfFile;
        private readonly ISourceList<SparseIndex> _indicies = new SourceList<SparseIndex>();

        public Encoding Encoding { get; }
        public FileInfo Info { get; }
        public int Compression { get;  }
        public int TailSize { get;  }
        public IObservable<SparseIndexCollection> Result { get; }
        
        public SparseIndexer([NotNull] FileInfo info,
            IObservable<Unit> refresher,
            int compression = 10,
            int tailSize = 1000000,
            Encoding encoding = null,
            IScheduler scheduler = null)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            scheduler = scheduler ?? Scheduler.Default;

            Info = info;
            Compression = compression;
            TailSize = tailSize;
            Encoding = encoding ?? info.GetEncoding();

            //0. create  a resulting index object from the collection of index fragments
            Result = _indicies
                .Connect()
                .Sort(SortExpressionComparer<SparseIndex>.Ascending(si => si.Start))
                .ToCollection()
                .Scan((SparseIndexCollection)null, (previous, notification) => new SparseIndexCollection(notification, previous, Encoding));

            //1. Calculate at which point the tail of the file is
            var startScanningAt = (int)Math.Max(0, info.Length - tailSize);
            _endOfFile = startScanningAt==0 ? 0 : (int)info.FindNextEndOfLinePosition(startScanningAt);
            
            if (Info.Length==0)
                _indicies.Add(new SparseIndex(0,0,1,0, IndexType.Tail));

            //2. Scan the tail [TODO: put _endOfFile into observable]
            var tailScanner = refresher
                .StartWith(Unit.Default)
                .Select(_ => ScanTail(_endOfFile))
                .Where(tail => tail != null)
                .Select(tail =>
                {
                    //cannot use scan because reading head may update the last head
                    var previous = _indicies.Items.FirstOrDefault(si => si.Type == IndexType.Tail);
                    return previous == null ? tail : new SparseIndex(tail, previous);
                })
                .Do(index=> _endOfFile= index.End)
                .Publish();

            var locker = new object();

            //3. Scan tail when we have the first result from the head
            var tailSubscriber = tailScanner
                .Synchronize(locker)
                .Subscribe(tail =>
                {
                    _indicies.Edit(innerList =>
                    {
                        var existing = innerList.FirstOrDefault(si => si.Type == IndexType.Tail);
                        if (existing != null) innerList.Remove(existing);
                        innerList.Add(tail);
                    });
                });

            ////4. Scan the remainder of the file when the tail has been scanned
            var headSubscriber = _endOfFile==0 
                ? Disposable.Empty
                : tailScanner.FirstAsync()
                .Synchronize(locker)
                .Subscribe(tail =>
                {
                    var estimateLines = EstimateNumberOfLines(tail, info);
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

            _cleanUp = new CompositeDisposable(_indicies,
                tailSubscriber,
                headSubscriber,
                tailScanner.Connect());
        }

        private int EstimateNumberOfLines(SparseIndex tail, FileInfo info)
        {
            //Calculate estimate line count
            var averageLineLength = tail.Size/tail.LineCount;
            var estimatedLines = (info.Length - tail.Size) /averageLineLength;
            return (int) estimatedLines;
        }

        private SparseIndex ScanTail(int start)
        {
            return Scan(start,-1, 1);
        }

        private SparseIndex Scan(int start, int end, int compression)
        {
            int count = 0;
            int lastPosition = 0;
            using (var stream = File.Open(Info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                int[] lines;
                using (var reader = new StreamReaderExtended(stream, Encoding, false))
                { 
                    stream.Seek(start, SeekOrigin.Begin);
                    if (reader.EndOfStream) return null;

                    lines = reader.ScanLines(compression, i => i, (line, position) =>
                    {
                        lastPosition = position;
                        count++;
                        return end!=-1 && lastPosition >= end;

                    }).ToArray();
                }
                return new SparseIndex(start, lastPosition, lines, compression, count, end==-1 ? IndexType.Tail : IndexType.Page);
            }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}