using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using DynamicData;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    /// <summary>
    /// Responsive and flexible file searching.
    /// See https://github.com/RolandPheasant/TailBlazer/issues/42
    /// </summary>
    public class FileSearcher: IDisposable
    {
        private readonly IScheduler _scheduler;
        private readonly Func<string, bool> _predicate;
        private readonly int _arbitaryNumberOfMatchesBeforeWeBailOutBecauseMemoryGetsHammered;
        private readonly IDisposable _cleanUp;

        private Encoding Encoding { get;  set; }
        private FileInfo Info { get;  set; }

        public IObservable<FileSearchResult> SearchResult { get;  }

        public FileSearcher([NotNull] IObservable<FileSegmentCollection> fileSegments, 
            [NotNull] Func<string, bool> predicate,
            int arbitaryNumberOfMatchesBeforeWeBailOutBecauseMemoryGetsHammered = 50000,
            Encoding encoding = null,
            IScheduler scheduler =null)
        {
            if (fileSegments == null) throw new ArgumentNullException(nameof(fileSegments));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            _predicate = predicate;
            _arbitaryNumberOfMatchesBeforeWeBailOutBecauseMemoryGetsHammered = arbitaryNumberOfMatchesBeforeWeBailOutBecauseMemoryGetsHammered;
            _scheduler = scheduler ?? Scheduler.Default;

            var shared = fileSegments.Replay(1).RefCount();

            var infoSubscriber = shared.Select(segments => segments.Info)
            //    .Take(1)
                .Subscribe(info =>
                {
                    Info = info;
                    Encoding = encoding ?? info.GetEncoding();
                });
            //Create a cache of segments which are to be searched
            var segmentCache = shared.Select(s => s.Segments)
                .ToObservableChangeSet(s => s.Key)
                .IgnoreUpdateWhen((current,previous)=>current==previous)
                .AsObservableCache();
            
            //manually maintained search results and status
            var searchData= new SourceCache<FileSegmentSearch, FileSegmentKey>(s=>s.Key);
            
            SearchResult = searchData.Connect()
                .Flatten()
                .Select(change=>change.Current)
                .Scan((FileSearchResult)null, (previous, current) => previous==null 
                                ? new FileSearchResult(current, Info, Encoding, _arbitaryNumberOfMatchesBeforeWeBailOutBecauseMemoryGetsHammered) 
                                : new FileSearchResult(previous, current, Info, Encoding, _arbitaryNumberOfMatchesBeforeWeBailOutBecauseMemoryGetsHammered))
                .StartWith(FileSearchResult.None)
                .Replay(1).RefCount();

            //initialise a pending state for all segments
            var loader = segmentCache.Connect()
                .Transform(fs => new FileSegmentSearch(fs))
                .WhereReasonsAre(ChangeReason.Add)
                .PopulateInto(searchData);

            //scan end of file, then tail
            var tailSearch = segmentCache.WatchValue(FileSegmentKey.Tail)
                .Scan((FileSegmentSearch) null, (previous, current) =>
                {
                    if (previous == null)
                    {
                        var result = Search(current.Start, current.End);
                        return new FileSegmentSearch(current, result);
                    }
                    else
                    {
                        var result = Search(previous.Segment.End, current.End);
                        return result == null ? previous : new FileSegmentSearch(previous, result);
                    }
                })
                .DistinctUntilChanged()
                .Publish();
            
            //start tailing
            var tailSubscriber = tailSearch.Subscribe(tail => searchData.AddOrUpdate(tail));

            //load the rest of the file segment by segment, reporting status after each search
            var headSubscriber = tailSearch.Take(1).WithContinuation(() =>
            {
                var locker = new object();

                return searchData.Connect(fss=>fss.Segment.Type == FileSegmentType.Head )
                    .Do(head => Debug.WriteLine(head.First().Current))
                    .WhereReasonsAre(ChangeReason.Add)  
                    .SelectMany(changes=>changes.Select(c=>c.Current).OrderByDescending(c=>c.Segment.Index).ToArray())
                    .ObserveOn(_scheduler)
                    .Synchronize(locker)
                    .Do(head => searchData.AddOrUpdate(new FileSegmentSearch(head.Segment,FileSegmentSearchStatus.Searching) ))
                   .Select(fileSegmentSearch =>
                   {
                       /*
                            This hack imposes a limitation on the number of items returned as memory can be 
                            absolutely hammered [I have seen 20MB memory when searching a 1 GB file - obviously not an option]
                           TODO: A proper solution. 
                                   1. How about index to file?
                                   2. Allow auto pipe of large files
                                   3. Allow user to have some control here
                       */

                       var sum = searchData.Items.Sum(fss => fss.Lines.Length);

                       if (sum >= _arbitaryNumberOfMatchesBeforeWeBailOutBecauseMemoryGetsHammered)
                       {
                           return new FileSegmentSearch(fileSegmentSearch,  FileSegmentSearchStatus.Complete);
                       }
                       var result = Search(fileSegmentSearch.Segment.Start, fileSegmentSearch.Segment.End);
                       return new FileSegmentSearch(fileSegmentSearch, result);
                   });
            })
           .Subscribe(head => searchData.AddOrUpdate(head));

            _cleanUp = new CompositeDisposable(
                segmentCache, 
                loader,
                tailSubscriber,
                headSubscriber,
                tailSearch.Connect(),
                infoSubscriber,
                searchData);
        }

        private FileSegmentSearchResult Search(long start, long end)
        {
            long lastPosition = 0;
            using (var stream = File.Open(Info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {

                if (stream.Length < start)
                {
                    start = 0;
                    end = stream.Length;
                }
                long[] lines;
                using (var reader = new StreamReaderExtended(stream, true))
                {
                    stream.Seek(start, SeekOrigin.Begin);
                    if (reader.EndOfStream)
                        return new FileSegmentSearchResult(start,end);

                    lines = reader.SearchLines(_predicate, i => i, (line, position) =>
                    {
                        lastPosition = position;//this is end of line po
                        return end != -1 && lastPosition > end;

                    }).ToArray();
                }
                return new FileSegmentSearchResult(start, lastPosition, lines);
            }
        }
        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}