using System;
using System.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public enum AutoTailReason
    {
        LoadTail,
        NewLines
    }

    public class AutoTailResponse : IEquatable<AutoTailResponse>
    {
        public TailInfo TailInfo { get; }
        public int PageSize { get; }
        public Line[] Lines { get; }
        public AutoTailReason Reason { get;  }
        public int Count => Lines.Length;

        public static readonly AutoTailResponse Empty = new AutoTailResponse();

        public AutoTailResponse(TailInfo tailInfo, int pageSize, Line[] lines, AutoTailReason reason)
        {
            PageSize = pageSize;
            TailInfo = tailInfo;
            Lines = lines;
            Reason = reason;
        }

        public AutoTailResponse()
        {
            TailInfo = TailInfo.None;
            PageSize = 0;
            Lines = new Line[0];
        }

        #region Equality

        public bool Equals(AutoTailResponse other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(TailInfo, other.TailInfo)
                   && PageSize == other.PageSize
                   && Lines.SequenceEqual(other.Lines);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AutoTailResponse)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TailInfo?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ PageSize;
                hashCode = (hashCode * 397) ^ (Lines?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(AutoTailResponse left, AutoTailResponse right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AutoTailResponse left, AutoTailResponse right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"Tail starts at: {TailInfo.TailStartsAt}, PageSize: {PageSize}, Reason: {Reason}, Count: {Count}";
        }
    }
}