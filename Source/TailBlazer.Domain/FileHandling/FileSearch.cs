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
        private readonly IObservableCache<SearchableSegment, int> _segments;
        private readonly IDisposable _cleanUp ;

        public FileSearch([NotNull] IObservable<FileSegments> segments, [NotNull] Func<string, bool> predicate,IScheduler scheduler =null)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            _predicate = predicate;
            _scheduler = scheduler ?? Scheduler.Default;

            var shared = segments.Publish();
            var loader = shared.FirstAsync().Subscribe(s => Info = s.Info);

            //these are the elements which ca be searched
            _segments = shared.Select(s => s.Segments)
                .ToObservableChangeSet(s => s.Index)
                .Transform(segment=> new SearchableSegment(segment))
                .AsObservableCache();

            //1. Search tail always: find a way to directly watch the SearchableSegment for the tail only
            var tailSearch = _segments.Connect()
                .QueryWhenChanged(query =>
                {
                    return query.Items.OrderByDescending(seg => seg.Segment.Index).First();
                })
                .Scan((SearchableSegment)null, (previous, current) =>
                {
                    //problem here is each SearchableSegment is replaced when the head updates,
                    // so we probably need to maintain a seperate cahce results and manually
                    //manage each search

                    //In which case we need 2 search threads
                    //1. Tail search always
                    //2. Queue of items to be searched
                    //3. When results are obtained, update destination cache.
                    return null;
                })   
                .Subscribe();

            //search remainder of the file in predeterminied order and take one step at a time
            var body = shared.Select(s => s.Tail)
                .Subscribe();

            _cleanUp = new CompositeDisposable(shared.Connect(), _segments, loader);
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

        private class SearchableSegment
        {
            public FileSegment Segment { get; }

            public SearchableSegment(FileSegment segment)
            {
                Segment = segment;
            }

          //  public 

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