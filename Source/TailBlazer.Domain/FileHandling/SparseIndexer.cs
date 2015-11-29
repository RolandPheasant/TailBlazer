using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public class SparseIndexer: IDisposable
    {
        private readonly IDisposable _cleanUp;

        public Encoding Encoding { get; }
        public FileInfo Info { get; }
        public int Compression { get; set; }
        public int TailSize { get;  }
        

        private int _endOfFile;

        private readonly ISourceList<SparseIndex> _indicies= new SourceList<SparseIndex>();

        public IObservable<SparseIndicies> Result { get; }


        public SparseIndexer([NotNull] FileInfo info,
            IObservable<Unit> refresher,
            int compression = 10,
            int tailSize = 100000,
            int pageSize = 1000000,
            Encoding encoding = null,
            IScheduler scheduler = null)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            scheduler = scheduler ?? Scheduler.Default;

            Info = info;
            Compression = compression;
            TailSize = tailSize;
            Encoding = encoding ?? info.GetEncoding();

            //create  a resulting index object from the collection of index fragments
            Result = _indicies
                .Connect()
                .Sort(SortExpressionComparer<SparseIndex>.Ascending(si => si.Start))
                .ToCollection()
                .Scan((SparseIndicies)null, (previous, notification) =>
                {
                    return new SparseIndicies(notification, previous, Encoding);
                });


            //1. Get  full length of file
            var startScanningAt = (int)Math.Max(0, info.Length - tailSize);
            _endOfFile = startScanningAt;

            //2. Scan the tail [TODO: put _endOfFile into observable]
            var tailScanner = refresher
                .StartWith(Unit.Default)
                .Select(_ => ScanTail(_endOfFile))
                .Where(tail => tail != null)
                .Scan((SparseIndex) null, (previous, latest) =>
                {
                    return previous == null ? latest : new SparseIndex(latest, previous);
                })
                .Do(index=> _endOfFile= index.End)
                .Publish();


            //3. Scan tail when we have the first result from the head
            var tailSubscriber = tailScanner
               
                .Subscribe(tail =>
                {
                    _indicies.Edit(innerList =>
                    {
                        var existing = innerList.FirstOrDefault(si => si.Type == SpareIndexType.Tail);
                        if (existing != null) _indicies.Remove(existing);
                        _indicies.Add(tail);
                    });
                });

            ////3. Scan the remainder of the file when the first one has started
           //scheduler.Schedule(() =>
           //{

           //})

            var xxx = tailScanner.FirstAsync()
                .Subscribe(head =>
                {
                    var estimateLines = EstimateNumberOfLines(head, info);
                    var estimate = new SparseIndex(0, head.Start, compression, estimateLines, SpareIndexType.Page);
                    _indicies.Add(estimate);

                    scheduler.Schedule(() =>
                    {
                        var actual = Scan(0, head.Start, compression);
                        _indicies.Edit(innerList =>
                        {
                            innerList.Remove(estimate);
                            innerList.Add(actual);
                        });
                    });
                });




            _cleanUp = new CompositeDisposable(_indicies,
                tailSubscriber,
                tailScanner.Connect());
        }

        private  IObservable<SparseIndex> ScanHead(SparseIndex head,FileInfo info, int compression)
        {
            return Observable.Create<SparseIndex>(observer =>
            {
                var estimateLines = EstimateNumberOfLines(head, info);
                var estimate = new SparseIndex(0, head.Start, compression, estimateLines, SpareIndexType.Page);
                observer.OnNext(estimate);

                var scan = Scan(0, head.Start, compression);
                observer.OnNext(estimate);

                return Observable.StartAsync( () =>  ScanAsync(0, head.Start, compression))
                        .SubscribeSafe(observer);
            });
        }

        private int EstimateNumberOfLines(SparseIndex tail, FileInfo info)
        {
            //Calculate estimate line count
            var averageLineLength = tail.Size/tail.LineCount;
            var estimatedLines = info.Length/averageLineLength;
            return (int) estimatedLines;
        }

        private SparseIndex ScanTail(int start)
        {
            return Scan(start,-1, 1);
        }

        private async Task<SparseIndex> ScanAsync(int start, int end, int compression)
        {
            return  await Task.Run(()=> Scan( start,  end,  compression));
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
                if (lastPosition > end)
                {
                    //we have an overlapping line [must remove the last one from the head]
                }
                return new SparseIndex(start, lastPosition, lines, compression, count, end==-1 ? SpareIndexType.Tail : SpareIndexType.Page);
            }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}