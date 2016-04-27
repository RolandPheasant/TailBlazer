using System;
using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling
{
    public class ScrollRequest : IEquatable<ScrollRequest>
    {
        public static readonly ScrollRequest None= new ScrollRequest();

        public int PageSize { get;  }
        public int FirstIndex { get;  }

        public long Position { get; }

        public bool SpecifiedByPosition { get;  }

        public ScrollReason Mode { get; }
        
        public ScrollRequest(int pageSize)
        {
            PageSize = pageSize;
            Mode = ScrollReason.Tail;
        }

        private ScrollRequest()
        {
        }

        public ScrollRequest(int pageSize, long position)
        {
            PageSize = pageSize;
            Position = position;
            SpecifiedByPosition = true;
            Mode = ScrollReason.User;
        }

        public ScrollRequest(int pageSize, int firstIndex, bool specifiedByPosition=false)
        {
            PageSize = pageSize;
            FirstIndex = firstIndex;
            SpecifiedByPosition = specifiedByPosition;
            Mode = ScrollReason.User;
        }

        public ScrollRequest(ScrollReason mode, int pageSize, long position)
        {
            PageSize = pageSize;
            Mode = mode;
            Position = position;
            SpecifiedByPosition = true;
        }

        public ScrollRequest(ScrollReason mode, int pageSize, int firstIndex)
        {
            PageSize = pageSize;
            FirstIndex = firstIndex;
            Mode = mode;
            SpecifiedByPosition = false;
        }

        #region Equality

        public bool Equals(ScrollRequest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (Mode == ScrollReason.Tail)
                return PageSize == other.PageSize && Mode == other.Mode;

            return PageSize == other.PageSize
                && FirstIndex == other.FirstIndex
                 && Position == other.Position
                && Mode == other.Mode;
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ScrollRequest) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = PageSize;
                hashCode = (hashCode*397) ^ FirstIndex;
                hashCode = (hashCode*397) ^ Position.GetHashCode();
                hashCode = (hashCode*397) ^ SpecifiedByPosition.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Mode;
                return hashCode;
            }
        }

        public static bool operator ==(ScrollRequest left, ScrollRequest right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ScrollRequest left, ScrollRequest right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}