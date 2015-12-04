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
    //1. produce list of searchable segments
    //2. Order them to search - end segments first 
    //3. On search complete find next segment to search on.
    //4. User should be able to cancel / resume any time
    //5  Before and after each search provide user with some feedback
    //6. Always have an active search for the tail item
    //7. Maybe a way to limit the number of matched items

    /// <summary>
    /// Fast and flexible file searching:
    /// 
    /// See https://github.com/RolandPheasant/TailBlazer/issues/42
    /// </summary>
    public class FileSearch: IDisposable
    {
        public FileInfo Info { get; private set; }

        private readonly IScheduler _scheduler;
        private readonly Func<string, bool> _predicate;

        private readonly IDisposable _cleanUp ;
        public IObservable<FileSearchResult> Result { get; set; }
        public FileSearch([NotNull] IObservable<FileSegmentCollection> segments, [NotNull] Func<string, bool> predicate,IScheduler scheduler =null)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            _predicate = predicate;
            _scheduler = scheduler ?? Scheduler.Default;

            var shared = segments.Publish();
            var infoLoader = shared.FirstAsync().Subscribe(s => Info = s.Info);

            //Create a list of elements which are to be searched
            var segmentCache = shared.Select(s => s.Segments)
                .ToObservableChangeSet(s => s.Key)
                .IgnoreUpdateWhen((current,previous)=>current==previous)
                .AsObservableCache();
            
            var searchData= new SourceCache<FileSegmentSearch, FileSegmentKey>(s=>s.Key);
            
            Result = searchData.Connect().QueryWhenChanged(query => new FileSearchResult(query.Items));

            //initialise a pending state for all segments
            var loader = segmentCache.Connect()
                .Transform(fs => new FileSegmentSearch(fs))
                .PopulateInto(searchData);

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
                        if (result == null) return previous;
                        return new FileSegmentSearch(previous, result);
                    }
                })
                .DistinctUntilChanged()
                .Publish();
            
            var tailSubscriber = tailSearch
                                .Subscribe(tail => searchData.AddOrUpdate(tail));

            var headSubscriber = tailSearch.Take(1).ContinueAfter(() =>
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



        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
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