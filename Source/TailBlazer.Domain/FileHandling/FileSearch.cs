using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Aggregation;
using TailBlazer.Domain.Annotations;

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
        private readonly IObservableCache<FileSegment, FileSegmentKey> _segments;
        private readonly IDisposable _cleanUp ;
        public IObservable<bool> IsSearching { get; }

        public FileSearch([NotNull] IObservable<FileSegments> segments, [NotNull] Func<string, bool> predicate,IScheduler scheduler =null)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            _predicate = predicate;
            _scheduler = scheduler ?? Scheduler.Default;

            var shared = segments.Publish();
            var infoLoader = shared.FirstAsync().Subscribe(s => Info = s.Info);

            //Create a list of elements which are to be searched
            _segments = shared.Select(s => s.Segments)
                .ToObservableChangeSet(s => s.Key)
                .IgnoreUpdateWhen((current,previous)=>current==previous)
                .AsObservableCache();


            var searchData= new SourceCache<FileSegmentSearch, FileSegmentKey>(s=>s.Key);
            
            IsSearching = searchData.Connect()
                .QueryWhenChanged(query =>
                {
                    return query.Items.Any(search => search.Status == FileSegmentSearchStatus.Searching);
                }).DistinctUntilChanged();

            //initialise a pending state for all segments
            var loader = _segments.Connect()
                .Transform(fs => new FileSegmentSearch(fs))
                .PopulateInto(searchData);

            var tailSearch = _segments.WatchValue(FileSegmentKey.Tail)
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
                        return new FileSegmentSearch(previous, result);
                    }
                }).Subscribe(tail=> searchData.AddOrUpdate(tail));
                                

            _cleanUp = new CompositeDisposable(shared.Connect(), _segments, loader, tailSearch, infoLoader);
        }



        public IObservable<FileSearchResult> Search()
        {
            return Observable.Create<FileSearchResult>(observer =>
            {
                //1. Segment files: Scan tail first
                //2. Provide user feedback?

                _scheduler.Schedule(() =>
                {
                    var searchResult= Search(0, Info.Length);
                });

                return new CompositeDisposable();
            });
        }



        private FileSearchResult Search(long start, long end)
        {
            long lastPosition = 0;
            using (var stream = File.Open(Info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                long[] lines;
                using (var reader = new StreamReaderExtended(stream, true))
                {
                    stream.Seek(start, SeekOrigin.Begin);
                    if (reader.EndOfStream)
                        return null;

                    lines = reader.SearchLines(_predicate, i => i, (line, position) =>
                    {
                        lastPosition = position;
                        return end != -1 && lastPosition >= end;

                    }).ToArray();
                }
                return new FileSearchResult(start, lastPosition, lines);
            }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}