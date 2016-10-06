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
        private readonly IDisposable _cleanUp;

        public IObservableCache<Line, LineKey> Lines { get; } 

        public IObservable<int> TotalLines { get; }

        public IObservable<long> Size { get; }


        public FileMonitor([NotNull] IObservable<FileSegmentsWithTail> fileSegments,
            IObservable<ScrollRequest> scrollRequest,
            int compression = 10,
            int sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing = 250000000,
            IScheduler scheduler = null)
        {
            if (fileSegments == null) throw new ArgumentNullException(nameof(fileSegments));
            
            var cache = new SourceCache<Line, LineKey>(l => l.Key);
            Lines = cache.AsObservableCache();

            var locker = new object();

            //set up synchronised scrolling
            var published = fileSegments.Synchronize(locker).Publish();
            var scroll = scrollRequest.Synchronize(locker);
            var indexer = new Indexer(published, compression, sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing, scheduler)
                                .Result
                                .Do(x=> {},ex=>{Console.WriteLine(ex);})
                                .Synchronize(locker);

            //set up file info observables
            var nameChanged = published.Select(fsc => fsc.Segments.Info.Name).DistinctUntilChanged().Skip(1).ToUnit();
            var sizeChanged = published.Select(fsc => fsc.Segments.SizeDiff).Where(sizeDiff => sizeDiff < 0).ToUnit();
            var tail = published.Select(fsc => fsc.TailInfo).DistinctUntilChanged();
            var invalidated = nameChanged.Merge(sizeChanged);

            TotalLines = indexer.Select(idx => idx.Count).Replay(1).RefCount();
            Size = published.Select(fsc => fsc.Segments.SizeDiff).Replay(1).RefCount(); 

            //keep monitoring until the file has been invalidated i.e. rollover or file name changed
            var  scrollInfo = indexer
                .CombineLatest(scroll, tail, (idx, scrl, t) => new IndiciesWithScroll(idx, scrl, t))
                .Scan((IndiciesWithScroll)null,(state,latest)=> state==null 
                            ? new IndiciesWithScroll(latest.IndexCollection,latest.Scroll, latest.TailInfo) 
                            : new IndiciesWithScroll(state,latest.IndexCollection, latest.Scroll, latest.TailInfo))
                .StartWith(IndiciesWithScroll.Empty)
                .TakeUntil(invalidated);

            var monitor = PopulateData(cache, scrollInfo);

            _cleanUp = new CompositeDisposable(published.Connect(), Lines, cache, monitor);
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
                        var result = latest.IndexCollection.ReadLines(latest.Scroll).ToArray();
                        innerCache.Clear();
                        innerCache.AddOrUpdate(result);
                    }
                    else if (latest.Reason == IndiciesWithScrollReason.TailChanged)
                    {
                        //only load data if tailing or there is available spacce on the pages
                        if (latest.Scroll.Mode == ScrollReason.Tail)
                        {
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
                        var result = latest.IndexCollection.ReadLines(latest.Scroll).ToArray();

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

        public enum IndiciesWithScrollReason
        {
            New,
            InitialLoad,
            TailChanged,
            IndexChanged,
            ScrollChanged
        }


        private class IndiciesWithScroll : IEquatable<IndiciesWithScroll>
        {
            public ScrollRequest Scroll { get;  }
            public TailInfo TailInfo { get; }
            public FileIndexCollection IndexCollection { get; }

            public bool PageSizeChanged { get; }
            public IndiciesWithScrollReason Reason { get; }

            public static readonly IndiciesWithScroll Empty = new IndiciesWithScroll();

            public IndiciesWithScroll(FileIndexCollection indexCollection, ScrollRequest scroll, TailInfo tail)
            {
                Scroll = scroll;
                TailInfo = tail;
                IndexCollection = indexCollection;
                PageSizeChanged = false;
                Reason = IndiciesWithScrollReason.InitialLoad;
            }

            public IndiciesWithScroll(IndiciesWithScroll previous,  FileIndexCollection indexCollection, ScrollRequest scroll, TailInfo tail)
            {
                Scroll = scroll;
                TailInfo = tail;
                IndexCollection = indexCollection;
                PageSizeChanged = previous.Scroll.PageSize != scroll.PageSize;

                if (indexCollection.Diff < 0)
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
                return Equals(Scroll, other.Scroll) && Equals(TailInfo, other.TailInfo) && Equals(IndexCollection, other.IndexCollection);
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
                    hashCode = (hashCode*397) ^ (IndexCollection != null ? IndexCollection.GetHashCode() : 0);
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