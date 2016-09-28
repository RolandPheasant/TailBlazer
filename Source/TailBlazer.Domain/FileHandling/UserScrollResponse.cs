using System;
using System.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public class UserScrollResponse : IEquatable<UserScrollResponse>
    {
        public TailInfo TailInfo { get; }
        public int PageSize { get; }
        public int FirstIndex { get; set; }
        public Line[] Lines { get; }

        public int Count => Lines.Length;

        public static readonly UserScrollResponse Empty = new UserScrollResponse();

        public UserScrollResponse(TailInfo tailInfo, int pageSize, int firstIndex, Line[] lines)
        {
            PageSize = pageSize;
            FirstIndex = firstIndex;
            TailInfo = tailInfo;
            Lines = lines;
        }

        public UserScrollResponse()
        {
            TailInfo = TailInfo.None;
            PageSize = 0;
            FirstIndex = 0;
            Lines = new Line[0];
        }

        #region Equality

        public bool Equals(UserScrollResponse other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(TailInfo, other.TailInfo)
                   && PageSize == other.PageSize
                   && FirstIndex == other.FirstIndex
                   && Lines.SequenceEqual(other.Lines);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UserScrollResponse)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TailInfo?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ PageSize;
                hashCode = (hashCode * 397) ^ FirstIndex;
                hashCode = (hashCode * 397) ^ (Lines?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(UserScrollResponse left, UserScrollResponse right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(UserScrollResponse left, UserScrollResponse right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"Tail starts at: {TailInfo.TailStartsAt}, PageSize: {PageSize}, FirstIndex: {FirstIndex}, Count: {Count}";
        }
    }
}