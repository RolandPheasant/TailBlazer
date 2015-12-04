using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    /// <summary>
    /// Responsive and flexible file searching.
    /// 
    /// See https://github.com/RolandPheasant/TailBlazer/issues/42
    /// </summary>
    public class FileSearch: IDisposable
    {
        private readonly IScheduler _scheduler;
        private readonly Func<string, bool> _predicate;
        private readonly IDisposable _cleanUp ;

        private FileInfo Info { get; set; }
        public IObservable<FileSearchResult> SearchResult { get;  }

        public FileSearch([NotNull] IObservable<FileSegmentCollection> segments, [NotNull] Func<string, bool> predicate,IScheduler scheduler =null)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            _predicate = predicate;
            _scheduler = scheduler ?? Scheduler.Default;

            var shared = segments.Publish();
            var infoLoader = shared.FirstAsync().Subscribe(s => Info = s.Info);

            //Create a cache of segments which are to be searched
            var segmentCache = shared.Select(s => s.Segments)
                .ToObservableChangeSet(s => s.Key)
                .IgnoreUpdateWhen((current,previous)=>current==previous)
                .AsObservableCache();
            
            //manually maintained search results and status
            var searchData= new SourceCache<FileSegmentSearch, FileSegmentKey>(s=>s.Key);
            
            //this is the result which forms the API
            SearchResult = searchData.Connect()
                        .QueryWhenChanged(query => new FileSearchResult(query.Items))
                        .StartWith(FileSearchResult.None);

            //initialise a pending state for all segments
            var loader = segmentCache.Connect()
                .Transform(fs => new FileSegmentSearch(fs))
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
            var tailSubscriber = tailSearch
                                .Subscribe(tail => searchData.AddOrUpdate(tail));

            //load the rest of the file segment by segment, reporting status after each search
            var headSubscriber = tailSearch.Take(1).WithContinuation(() =>
            {
                var locker = new object();
                return searchData
                    .Connect(sd => sd.Segment.Type == FileSegmentType.Head && sd.Status == FileSegmentSearchStatus.Pending)
                    .SelectMany(changes=>changes.Select(c=>c.Current).OrderByDescending(c=>c.Segment.Index).ToArray())
                    .ObserveOn(_scheduler)
                    .Synchronize(locker)
                    .Do(head => searchData.AddOrUpdate(new FileSegmentSearch(head.Segment,FileSegmentSearchStatus.Searching) ))
                    .Select(fss =>
                    {
                        var result = Search(fss.Segment.Start, fss.Segment.End);
                        return new FileSegmentSearch(fss, result);
                    });
            })
           .Subscribe(head => searchData.AddOrUpdate(head));

            _cleanUp = new CompositeDisposable(shared.Connect(),
                segmentCache, 
                loader,
                tailSubscriber,
                headSubscriber,
                tailSearch.Connect(),
                infoLoader);
        }

        private FileSegmentSearchResult Search(long start, long end)
        {
            long lastPosition = 0;
            using (var stream = File.Open(Info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                long[] lines;
                using (var reader = new StreamReaderExtended(stream, true))
                {
                    stream.Seek(start, SeekOrigin.Begin);
                    if (reader.EndOfStream)
                        return new FileSegmentSearchResult(start,end);

                    lines = reader.SearchLines(_predicate, i => i, (line, position) =>
                    {
                        lastPosition = position;
                        return end != -1 && lastPosition >= end;

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