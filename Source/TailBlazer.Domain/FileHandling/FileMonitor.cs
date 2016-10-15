using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
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
        public IObservable<long> Size { get; }
        
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

            var shared = Monitor().Publish();
            
            Size = shared.Select(obs => (long)obs.LineReader.Count).DistinctUntilChanged().Replay(1).RefCount();
            TotalLines = shared.Select(obs => obs.LineReader.Count).DistinctUntilChanged().Replay(1).RefCount();

            var monitor = shared.Subscribe(obs=>  PopulateData(cache, obs)) ;

            _cleanUp = new CompositeDisposable(shared.Connect(), monitor,  Lines, cache);
        }

        private IObservable<IndiciesWithScroll> Monitor()
        {
            return _predicates.Select(predicate =>
            {
                return _fileSegments.Publish(shared =>
                {
                    var locker = new object();
                    var scroll = _scrollRequest.DistinctUntilChanged().Synchronize(locker);
                    var indexer = shared.Monitor(predicate, _scheduler).Synchronize(locker);

                    ////set up file info observables
                    var tail = shared.Select(fsc => fsc.TailInfo).DistinctUntilChanged();

                    return indexer
                        .CombineLatest(scroll, tail, (idx, scrl, t) => new IndiciesWithScroll(idx, scrl, t))
                        .Scan((IndiciesWithScroll) null, (state, latest) =>
                        {
                            return state == null
                                ? new IndiciesWithScroll(latest.LineReader, latest.Scroll, latest.TailInfo)
                                : new IndiciesWithScroll(state, latest.LineReader, latest.Scroll, latest.TailInfo);
                        })
                        .StartWith(IndiciesWithScroll.Empty);
                });
            }).Switch();
        }

        private void PopulateData(SourceCache<Line, LineKey> cache, IndiciesWithScroll latest)
        {
            //load the cache with data matching the scroll request
            cache.Edit(innerCache =>
            {
                if (latest.Reason == IndiciesWithScrollReason.New)
                {
                    innerCache.Clear();
                }
                else if (latest.Reason == IndiciesWithScrollReason.InitialLoad)
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
                else if (latest.Reason == IndiciesWithScrollReason.TailChanged)
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
                else if (latest.Reason == IndiciesWithScrollReason.ScrollChanged)
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


        private enum IndiciesWithScrollReason
        {
            None,
            New,
            InitialLoad,
            TailChanged,
            IndexChanged,
            ScrollChanged
        }

        private class MonitorObservables
        {
            public IObservable<int> TotalLines { get; }
            public IObservable<long> Size { get; }
            public IObservable<IndiciesWithScroll> ScrollInfo { get; }

            public MonitorObservables(IObservable<IndiciesWithScroll> scrollInfo, IObservable<int> totalLines, IObservable<long> size)
            {
                ScrollInfo = scrollInfo;
                TotalLines = totalLines;
                Size = size;
            }
        }



        private class IndiciesWithScroll : IEquatable<IndiciesWithScroll>
        {
            public ScrollRequest Scroll { get;  }
            public TailInfo TailInfo { get; }
            public ILineReader LineReader { get; }

            public bool PageSizeChanged { get; }
            public IndiciesWithScrollReason Reason { get; }

            public static readonly IndiciesWithScroll Empty = new IndiciesWithScroll();

            public IndiciesWithScroll(ILineReader lineReader, ScrollRequest scroll, TailInfo tail)
            {

                Scroll = scroll;
                //if the line reader provides it's own tail info, then use it
                TailInfo = (lineReader as IHasTailInfo)?.TailInfo ?? tail;
                LineReader = lineReader;
                PageSizeChanged = false;
                Reason = IndiciesWithScrollReason.InitialLoad;
            }

            public IndiciesWithScroll(IndiciesWithScroll previous, ILineReader lineReader, ScrollRequest scroll, TailInfo tail)
            {
                Scroll = scroll;
                TailInfo = tail;
                LineReader = lineReader;
                PageSizeChanged = previous.Scroll.PageSize != scroll.PageSize;




                if (lineReader.Count ==0 ||  lineReader.Count < previous.LineReader.Count)
                {
                    Reason = IndiciesWithScrollReason.InitialLoad;
                }

                else if (tail != previous.TailInfo)
                {
                    Reason = IndiciesWithScrollReason.TailChanged;
                }
                else if (scroll!= previous.Scroll)
                {
                    Reason = IndiciesWithScrollReason.ScrollChanged;
                }
                else
                {
                    Reason = IndiciesWithScrollReason.IndexChanged;
                }

            }

            private IndiciesWithScroll()
            {
                Reason = IndiciesWithScrollReason.New;
                LineReader = NullLineReader.Empty;
            }

            #region Equality

            public bool Equals(IndiciesWithScroll other)
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
                return Equals((IndiciesWithScroll) obj);
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

            public static bool operator ==(IndiciesWithScroll left, IndiciesWithScroll right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(IndiciesWithScroll left, IndiciesWithScroll right)
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