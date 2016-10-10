using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    public class FileMonitor: ILineMonitor, IDisposable
    {
        private readonly IObservable<FileSegmentsWithTail> _fileSegments;
        private readonly IObservable<ScrollRequest> _scrollRequest;
        private readonly IDisposable _cleanUp;

        public IObservableCache<Line, LineKey> Lines { get; } 
        public IObservable<int> TotalLines { get; }
        public IObservable<long> Size { get; }
        
        public FileMonitor([NotNull] IObservable<FileSegmentsWithTail> fileSegments,
            [NotNull] IObservable<ScrollRequest> scrollRequest,
            Func<string, bool> predicate = null,
            IObservable<Func<string, bool>> predicateObs = null,
            IScheduler scheduler = null)
        {
            if (fileSegments == null) throw new ArgumentNullException(nameof(fileSegments));
            if (scrollRequest == null) throw new ArgumentNullException(nameof(scrollRequest));

            _fileSegments = fileSegments;
            _scrollRequest = scrollRequest;
            
            var rolloverDisposable = new SerialDisposable();
            
            var cache = new SourceCache<Line, LineKey>(l => l.Key);
            Lines = cache.Connect()
                        .IgnoreUpdateWhen((current, previous) => current.Key == previous.Key)
                        .AsObservableCache();



            IObservable<Func<string, bool>> xxx;
            IObservable<MonitorObservables> observables;
            if (predicateObs != null)
            {


                var pred = predicateObs.Take(1).Wait();
                observables = CreateObservables(fsg => fsg.Monitor(pred, scheduler));
                //observables = predicateObs
                //    .Select(pred => CreateObservables(fsg => fsg.Monitor(pred, scheduler)))
                //    .Switch();
            }
            else
            {
                observables = CreateObservables(fsg => fsg.Monitor(predicate, scheduler));
            }

           // IObservable<MonitorObservables> observables = CreateObservables(fsg=> fsg.Monitor(predicate, scheduler));
            Size = observables.Select(obs => obs.Size).Switch();
            TotalLines = observables.Select(obs => obs.TotalLines).Switch();
            var monitor = observables.Subscribe(obs=> rolloverDisposable.Disposable = PopulateData(cache, obs.ScrollInfo)) ;

            _cleanUp = new CompositeDisposable(monitor, rolloverDisposable, Lines, cache);
        }

        private IObservable<MonitorObservables> CreateObservables(Func<IObservable<FileSegmentsWithTail>,IObservable<ILineReader>> lineReaderFactory)
        {
            return Observable.Create<MonitorObservables>(observer =>
            {
                var locker = new object();

                //ensure input streams are synchronised
                var published = _fileSegments.Synchronize(locker).Publish();
                var scroll = _scrollRequest.DistinctUntilChanged().Synchronize(locker);
                var indexer = lineReaderFactory(_fileSegments).Synchronize(locker);

                //set up file info observables
                var nameChanged = published.Select(fsc => fsc.Segments.Metrics.Name).DistinctUntilChanged().Skip(1).ToUnit();
                var sizeChanged = published.Select(fsc => fsc.Segments.SizeDiff).Where(sizeDiff => sizeDiff < 0).ToUnit();
                var tail = published.Select(fsc => fsc.TailInfo).DistinctUntilChanged();

                //kill the stream when file rolls over, or when the name has changed
                var invalidated = nameChanged.Merge(sizeChanged);

                var totalLines = indexer.Select(idx => idx.Count).Replay(1).RefCount();
                var size = published.Select(fsc => fsc.Segments.FileSize).Replay(1).RefCount();

                //keep monitoring until the file has been invalidated i.e. rollover or file name changed
                var scrollInfo = indexer
                    .CombineLatest(scroll, tail, (idx, scrl, t) => new IndiciesWithScroll(idx, scrl, t))
                    .Scan((IndiciesWithScroll) null, (state, latest) => state == null
                        ? new IndiciesWithScroll(latest.LineReader, latest.Scroll, latest.TailInfo)
                        : new IndiciesWithScroll(state, latest.LineReader, latest.Scroll, latest.TailInfo))
                    .StartWith(IndiciesWithScroll.Empty)
                    .TakeUntil(invalidated)
                    .FinallySafe(() =>     observer.OnCompleted()); 

                observer.OnNext(new MonitorObservables(scrollInfo, totalLines, size));

                return new CompositeDisposable(published.Connect());
            }).Repeat();
        }

        
        private IDisposable PopulateData(SourceCache<Line, LineKey> cache, IObservable<IndiciesWithScroll> scrollInfo)
        {
            //load the cache with data matching the scroll request
            var shared = scrollInfo.Publish();

            //tail file
            var loader = shared.Subscribe(latest =>
            {
                cache.Edit(innerCache =>
                {
                    if (latest.Reason == IndiciesWithScrollReason.New)
                    {
                        innerCache.Clear();
                    }
                    else if (latest.Reason == IndiciesWithScrollReason.InitialLoad)
                    {
                        var result = latest.LineReader.ReadLines(latest.Scroll).ToArray();
                        innerCache.Clear();
                        innerCache.AddOrUpdate(result);
                    }
                    else if (latest.Reason == IndiciesWithScrollReason.TailChanged)
                    {
                        //TODO: If in scroll mode, auto tail only if page is not full

                        //only load data if tailing or there is available spacce on the pages
                        if (latest.Scroll.Mode == ScrollReason.Tail)
                        {
                            if (latest.TailInfo.Count == 0) return;
                            innerCache.AddOrUpdate(latest.TailInfo.Lines);

                            //clear stale tail items
                            var toRemove = innerCache.Items
                                .OrderBy(l => l.LineInfo.Start)
                                .Take(cache.Count - latest.Scroll.PageSize)
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
            });
            return new CompositeDisposable(shared.Connect(), loader);
        }

        private enum IndiciesWithScrollReason
        {
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

                if (lineReader.Diff < 0)
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