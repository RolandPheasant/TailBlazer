using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Aggregation;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public class LinesController: ILineScroller
    {
        private readonly ISourceCache<Line, LineKey> _tail = new SourceCache<Line, LineKey>(lp => lp.Key);
        private readonly IDisposable _cleanUp;

        public IObservableCache<Line, LineKey> Lines => _tail;

        public LinesController([NotNull] IObservable<ILineProvider> lineProviderChanged, [NotNull] IObservable<ScrollRequest> scrollRequest)
        {
            //1. Split scroll between modes a) Tailing b) Sccrolling
            //2. Always maintain scroll cache and alway load from last position

            var locker = new object();

            scrollRequest = scrollRequest.Synchronize(locker);
            lineProviderChanged = lineProviderChanged.Synchronize(locker);

            const int pageFactor = 2;

            //tail a page twice the size of the one required
            var tailChanged = scrollRequest.Where(request => request.Mode == ScrollReason.Tail)
                .Select(request => new Dimensions(request.FirstIndex, pageFactor * request.PageSize))
                .DistinctUntilChanged();
            
            var tailer = lineProviderChanged
                .CombineLatest(tailChanged, (lineProvider, dimensions) => new { lineProvider, dimensions })
                .Sample(TimeSpan.FromMilliseconds(25))
                .Scan(LastRead.Empty, (state, latest) =>
                {
                    var lineProvider = latest.lineProvider;
                    var dimensions = latest.dimensions;

                    if (state == LastRead.Empty)
                    {
                        //load entire page
                        var lines = lineProvider.ReadLines(new ScrollRequest(dimensions.PageSize)).ToArray();
                        var firstIndex = lines.Select(lp => lp.Index).Max();
                        var lastIndex = lines.Select(lr => lr.LineInfo.Start).Max();
                        return new LastRead(firstIndex, lastIndex, lineProvider.Count, lines);

                    }
                    else
                    {
                        var toLoad = Math.Max(0, lineProvider.Count - state.Count);

                        //otherwise load new lines only
                        if (state.Count == lineProvider.Count || toLoad == 0)
                            return state;

                        var lines = lineProvider.ReadLines(new ScrollRequest(toLoad)).ToArray();
                        if (lines.Length == 0)
                            return state;

                        var firstIndex = lines.Select(lp => lp.Index).Max();
                        var lastIndex = lines.Select(lr => lr.LineInfo.Start).Max();
                        return new LastRead(firstIndex, lastIndex, lineProvider.Count, lines);
                    }
                }).DistinctUntilChanged()
                
                .Subscribe(lastRead =>
                {
                    //todo: Remove old items + Scroll to first item
                    _tail.AddOrUpdate(lastRead.Lines);
                });

            //    .ForAggregation()
            //    .Maximum(x=>x);

            _cleanUp = new CompositeDisposable(tailer, _tail);
        }



        public void Dispose()
        {
            _cleanUp.Dispose();
        }


        private class Dimensions
        {
            public int FirstIndex { get; }
            public int PageSize { get; }

            public Dimensions(int firstIndex, int pageSize)
            {
                FirstIndex = firstIndex;
                PageSize = pageSize;
            }
        }

        private class LastRead : IEquatable<LastRead>
        {
            public int FirstIndex { get; }
            public long LastLineRead { get; }
            public int Count { get; }

            public Line[] Lines { get; }

            public static readonly LastRead Empty = new LastRead(0, 0, 0, new Line[0]);

            public LastRead(int firstIndex, long lastLineRead, int count, Line[] lines)
            {
                FirstIndex = firstIndex;
                LastLineRead = lastLineRead;
                Count = count;
                Lines = lines;
            }

            #region equality

            public bool Equals(LastRead other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return FirstIndex == other.FirstIndex && Count == other.Count;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((LastRead)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (FirstIndex * 397) ^ Count;
                }
            }

            public static bool operator ==(LastRead left, LastRead right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(LastRead left, LastRead right)
            {
                return !Equals(left, right);
            }

            #endregion
        }
    }
}