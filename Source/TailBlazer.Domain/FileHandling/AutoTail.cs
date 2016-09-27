using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public class AutoTail: IAutoTail
    {
        private readonly IObservable<ILineProvider> _lineProvider;
        private readonly IObservable<int> _pageSize;

        public AutoTail([NotNull] IObservable<ILineProvider> lineProvider, [NotNull] IObservable<int> pageSize )
        {
            if (lineProvider == null) throw new ArgumentNullException(nameof(lineProvider));
            if (pageSize == null) throw new ArgumentNullException(nameof(pageSize));
            _lineProvider = lineProvider;
            _pageSize = pageSize;
        }

        public IObservable<IEnumerable<Line>> Tail()
        {

           return _lineProvider.CombineLatest(_pageSize, (lp, pge) => new { LineProvider = lp, PageSize = pge })
                    .Scan(LastScanInfo.Empty, (last, latest) =>
                    {
                        Line[] result;
                        if (last == LastScanInfo.Empty && last.PageSize == latest.PageSize && last.TailInfo.TailStartsAt == latest.LineProvider.TailInfo.TailStartsAt)
                            return last;

                        //if (last != LastScanInfo.Empty && last.PageSize == latest.PageSize && last.TailInfo.TailStartsAt == latest.LineProvider.TailInfo.TailStartsAt)
                        //    return last;

                        if (latest.PageSize != last.PageSize)
                        {
                            result = latest.LineProvider.ReadLines(new ScrollRequest(latest.PageSize)).ToArray();
                        }
                        else
                        {
                            result = latest.LineProvider.ReadLines(new ScrollRequest(ScrollReason.TailOnly, latest.PageSize, latest.LineProvider.TailInfo.TailStartsAt)).ToArray();
                        }
                        return new LastScanInfo(latest.LineProvider.TailInfo, latest.PageSize, result);
                    })
                    .DistinctUntilChanged()
                    .Where(result => result.Result.Length != 0)
                    .Select(info => info.Result);

        }

        private class LastScanInfo : IEquatable<LastScanInfo>
        {
            public TailInfo TailInfo { get;  }
            public int PageSize { get;  }
            public Line[] Result { get;  }

            public readonly static LastScanInfo Empty = new LastScanInfo(TailInfo.None,0, new Line[0]);

            public LastScanInfo(TailInfo tailInfo, int pageSize, Line[] result)
            {
                PageSize = pageSize;
                TailInfo = tailInfo;
                Result = result;
            }

            #region Equality

            public bool Equals(LastScanInfo other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(TailInfo, other.TailInfo) 
                    && PageSize == other.PageSize 
                    && Result.SequenceEqual(other.Result);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((LastScanInfo) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (TailInfo != null ? TailInfo.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ PageSize;
                    hashCode = (hashCode*397) ^ (Result != null ? Result.GetHashCode() : 0);
                    return hashCode;
                }
            }

            public static bool operator ==(LastScanInfo left, LastScanInfo right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(LastScanInfo left, LastScanInfo right)
            {
                return !Equals(left, right);
            }

            #endregion
        }
    }
}