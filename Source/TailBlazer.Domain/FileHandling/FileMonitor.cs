using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Kernel;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public class FileMonitor: ILineMonitor
    {
        private readonly IObservable<FileSegmentReport> _fileSegments;
        private readonly IObservable<ScrollRequest> _scrollRequest;
        private readonly IObservable<Func<string, bool>> _predicates;

        private readonly IScheduler _scheduler;
        private readonly IDisposable _cleanUp;

        public IObservableCache<Line, LineKey> Lines { get; } 
        public IObservable<int> TotalLines { get; }

        public IObservable<IProgressInfo> Progress { get; }

        public FileMonitor([NotNull] IObservable<FileSegmentReport> fileSegments,
            [NotNull] IObservable<ScrollRequest> scrollRequest,
            Func<string, bool> predicate = null,
            IObservable<Func<string, bool>> predicateObs = null,
            IScheduler scheduler = null)
        {
            if (fileSegments == null) throw new ArgumentNullException(nameof(fileSegments));
            if (scrollRequest == null) throw new ArgumentNullException(nameof(scrollRequest));

            _fileSegments = fileSegments;
            _scrollRequest = scrollRequest;
            _scheduler = scheduler ?? Scheduler.Default;
            _predicates = predicateObs ?? (Observable.Return(predicate));

            var cache = new SourceCache<Line, LineKey>(l => l.Key);

            Lines = cache.Connect()
                        .IgnoreUpdateWhen((current, previous) => current.Key == previous.Key)
                        .AsObservableCache();

            var shared = Monitor()
                .RetryWithBackOff<LineReaderInfo,Exception>((ex,i)=> TimeSpan.FromSeconds(1))
                .Publish();
            
            TotalLines = shared.Select(obs => obs.LineReader.Count)
                .DistinctUntilChanged()
                .Replay(1).RefCount();

            Progress = shared.Select(lri => lri.LineReader).OfType<IProgressInfo>()
                .Select(info=>new ProgressInfo(info.Completed, info.Total, info.IsSearching))
                .DistinctUntilChanged()
                .StartWith(ProgressInfo.Empty)
                .Replay(1).RefCount();

            var monitor = shared.Subscribe(obs=>  PopulateData(cache, obs)) ;

            _cleanUp = new CompositeDisposable(shared.Connect(), monitor,  Lines, cache);
        }

        private IObservable<LineReaderInfo> Monitor()
        {
            return _predicates.Select(predicate =>
            {
                return _fileSegments.Publish(shared =>
                {
                    //////Invoked at roll-over or file cleared
                    var newFileCreated = shared
                        .Select(fsr => fsr.Changes.Reason)
                        .DistinctUntilChanged()
                        .Where(reason => reason == FileNotificationReason.CreatedOrOpened)
                        .Skip(1);

                    //////return empty when file does not exists
                    //var whenEmpty = shared
                    //    .Where(fsr => !fsr.Changes.ExistsAndIsValid())
                    //    .Select(_ => LineReaderInfo.Empty);


                    var locker = new object();
                    var scroll = _scrollRequest.Synchronize(locker).DistinctUntilChanged();
                    var indexer = shared.Synchronize(locker).Monitor(predicate, _scheduler);


                    var indexedFiles = indexer

                        .CombineLatest(scroll, (idx, scrl) => new {LineReader = idx, Scroll = scrl})
                        .Scan((LineReaderInfo) null, (state, latest) =>
                        {
                            return state == null
                                ? new LineReaderInfo(latest.LineReader, latest.Scroll)
                                : new LineReaderInfo(state, latest.LineReader, latest.Scroll);
                        });
                           // .TakeUntil(newFileCreated)
                           // .Repeat();

                    return indexedFiles;
                   // return indexedFiles.Merge(whenEmpty).DistinctUntilChanged();
                    //   .StartWith(LineReaderInfo.Empty);
                });
            }).Switch();
        }

        private void PopulateData(SourceCache<Line, LineKey> cache, LineReaderInfo latest)
        {
            //load the cache with data matching the scroll request
            cache.Edit(innerCache =>
            {
                if (latest.Reason == LineReaderInfoChangedReason.New)
                {
                    innerCache.Clear();
                }
                else if (latest.Reason == LineReaderInfoChangedReason.InitialLoad)
                {

                    innerCache.Clear();


                    if (latest.TailInfo.Count != 0)
                    {
                        var size = latest.TailInfo.Count;
                        var pageSize = latest.Scroll.PageSize;
                        var toAdd = latest.TailInfo.Lines.Skip(size - pageSize);
                        innerCache.AddOrUpdate(toAdd);
                    }
                    else
                    {
                        var result = latest.LineReader.ReadLines(latest.Scroll).ToArray();
                        innerCache.AddOrUpdate(result);
                    }

                }
                else if (latest.Reason == LineReaderInfoChangedReason.TailChanged)
                {
                    //TODO: If in scroll mode, auto tail only if page is not full

                    //only load data if tailing or there is available spacce on the pages
                    if (latest.Scroll.Mode == ScrollReason.Tail)
                    {
                        if (latest.TailInfo.Count == 0) return;

                        var size = latest.TailInfo.Count;
                        var pageSize = latest.Scroll.PageSize;
                         var toAdd = latest.TailInfo.Lines.Skip(size - pageSize);
                        innerCache.AddOrUpdate(toAdd);

                        //clear stale tail items
                        var toRemove = innerCache.Items
                            .OrderBy(l => l.LineInfo.Start)
                            .Take(innerCache.Count - pageSize)
                            .Select(line => line.Key)
                            .ToArray();
                        innerCache.Remove(toRemove);
                    }

                }
                else if (latest.Reason == LineReaderInfoChangedReason.ScrollChanged)
                {
                    var result = latest.LineReader.ReadLines(latest.Scroll).ToArray();

                    var previous = innerCache.Items.ToArray();
                    var added = result.Except(previous, Line.TextStartComparer).ToArray();
                    var removed = previous.Except(result, Line.TextStartComparer).ToArray();

                    innerCache.AddOrUpdate(added);
                    innerCache.Remove(removed);
                }
            });
        }


        private enum LineReaderInfoChangedReason
        {
            None,
            New,
            InitialLoad,
            TailChanged,
            IndexChanged,
            ScrollChanged
        }

        private class LineReaderInfo : IEquatable<LineReaderInfo>
        {
            public ScrollRequest Scroll { get;  }
            public TailInfo TailInfo => LineReader.TailInfo;
            public ILineReader LineReader { get; }

            public bool PageSizeChanged { get; }
            public LineReaderInfoChangedReason Reason { get; }

            public static readonly LineReaderInfo Empty = new LineReaderInfo();

            public LineReaderInfo(ILineReader lineReader, ScrollRequest scroll)
            {
                Scroll = scroll;
                LineReader = lineReader;
                PageSizeChanged = false;
                Reason = LineReaderInfoChangedReason.InitialLoad;
            }

            public LineReaderInfo(LineReaderInfo previous, ILineReader lineReader, ScrollRequest scroll)
            {
                Scroll = scroll;
               // TailInfo = tail;
                LineReader = lineReader;
                PageSizeChanged = previous.Scroll.PageSize != scroll.PageSize;


                if (lineReader.Count ==0 ||  lineReader.Count < previous.LineReader.Count)
                    
                {
                    Reason = LineReaderInfoChangedReason.InitialLoad;
                }

                else if (lineReader.TailInfo != previous.LineReader.TailInfo)
                {
                    Reason = LineReaderInfoChangedReason.TailChanged;
                }
                else if (scroll!= previous.Scroll)
                {
                    Reason = LineReaderInfoChangedReason.ScrollChanged;
                }
                else
                {
                    Reason = LineReaderInfoChangedReason.IndexChanged;
                }

            }

            private LineReaderInfo()
            {
                Reason = LineReaderInfoChangedReason.New;
                LineReader = NullLineReader.Empty;
            }

            #region Equality

            public bool Equals(LineReaderInfo other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(Scroll, other.Scroll) && Equals(TailInfo, other.TailInfo) && Equals(LineReader, other.LineReader);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((LineReaderInfo) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (Scroll != null ? Scroll.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ (TailInfo != null ? TailInfo.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ (LineReader != null ? LineReader.GetHashCode() : 0);
                    return hashCode;
                }
            }

            public static bool operator ==(LineReaderInfo left, LineReaderInfo right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(LineReaderInfo left, LineReaderInfo right)
            {
                return !Equals(left, right);
            }

            #endregion
        }


        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}