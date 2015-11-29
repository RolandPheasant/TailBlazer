using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using DynamicData.Aggregation;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public class SparseIndicies
    {
        public Encoding Encoding { get; }
        public int LineFeedSize { get; }
        public SparseIndex[] Lines { get; }
        public int Count => Lines.Length;
        public int Diff { get; }
        public LinesChangedReason ChangedReason { get; }
        public int TailStartsAt { get; }


        public SparseIndicies(SparseIndex[] lines, Encoding encoding, int lineFeedSize, LineIndicies previous = null)
        {
            Encoding = encoding;
            LineFeedSize = lineFeedSize;
            if (previous == null)
            {
                Lines = lines;
                Diff = lines.Length;
                ChangedReason = LinesChangedReason.Loaded;
                TailStartsAt = lines.Length - 1;
            }
            else
            {
                //combine the 2 arrays
                //var latest = new int[previous.Lines.Length + lines.Length];
                //previous.Lines.CopyTo(latest, 0);
                //lines.CopyTo(latest, previous.Lines.Length);

               // Lines = latest;
                Diff = lines.Length;
                ChangedReason = LinesChangedReason.Tailed;
                TailStartsAt = previous.Count - 1;
            }
        }
    }


    public class SparseIndexer: IDisposable
    {
        private readonly IDisposable _cleanUp;

        public Encoding Encoding { get; }
        public FileInfo Info { get; }
        public int Compression { get; set; }
        public int TailSize { get;  }

        private int _endOfFile;

        private readonly ISourceList<SparseIndex> _indicies= new SourceList<SparseIndex>();
        public IObservableList<SparseIndex> Indicies { get; }

        public SparseIndexer([NotNull] FileInfo info,
            int compression = 10, 
            int tailSize=100000,
            int pageSize = 1000000,
            Encoding encoding = null,
            IScheduler scheduler=null)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            scheduler = scheduler ?? Scheduler.Default;

            Info = info;
            Compression = compression;
            TailSize = tailSize;
            Encoding = encoding ?? info.GetEncoding();

            Indicies = _indicies
                .Connect()
                .Sort(SortExpressionComparer<SparseIndex>.Ascending(si => si.Start))
                .AsObservableList();


            //1. Get  full length of file
            var startScanningAt = (int)Math.Max(0, info.Length - tailSize);
            _endOfFile = startScanningAt;

             //2. load tail
            Refresh();

            //enter blank initial index for start of file
            var lineCount = ExtimateNumberOfLines(Indicies.Items.ElementAt(0), info);
            _indicies.Add(new SparseIndex(0, startScanningAt, compression, lineCount));

            //scan the resmaining of the file
            //TODO: Partition this into pages, then can multi thread 
            scheduler.Schedule(() =>
            {
                var head = Scan(0, startScanningAt, compression);
                _indicies.ReplaceAt(1, head);
            });

            _cleanUp = new CompositeDisposable( _indicies,Indicies);
        }

        private int ExtimateNumberOfLines(SparseIndex tail, FileInfo info)
        {
            //Calculate estimate line count
            var averageLineLength = tail.Size / tail.LineCount;
            var estimatedLines = info.Length / averageLineLength;
            return (int)estimatedLines;
        }

        public void Refresh()
        {
            var tail = ScanTail(_endOfFile);
            _indicies.Add(tail);
            _endOfFile = tail.End;
        }

        public void LoadRemainingFile()
        {
            var tail = ScanTail(_endOfFile);
            _indicies.Add(tail);
            _endOfFile = tail.End;
        }

        public async Task<SparseIndex> ScanTailAsync(int start)
        {
            return await Task.Run(() => ScanTail(start));
        }

        public SparseIndex ScanTail(int start)
        {
            return Scan(start,-1, 1);
        }

        public async Task<SparseIndex> ScanAsync(int start, int end, int compression)
        {
            return  await Task.Run(()=> Scan( start,  end,  compression));
        }

        public  SparseIndex Scan(int start, int end, int compression)
        {
            int count = 0;
            int lastPosition = 0;
            using (var stream = File.Open(Info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                int[] lines;
                using (var reader = new StreamReaderExtended(stream, Encoding, false))
                {
                    stream.Seek(start, SeekOrigin.Begin);

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
                return new SparseIndex(start, lastPosition, lines, compression, count);
            }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}