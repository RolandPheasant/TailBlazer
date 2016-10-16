using System;

namespace TailBlazer.Domain.FileHandling
{
    public interface IProgressInfo
    {
        int Completed { get; }
        int Total { get; }
        bool IsSearching { get; }
    }


    public class ProgressInfo : IProgressInfo, IEquatable<ProgressInfo>
    {
        public int Completed { get; }
        public int Total { get; }
        public bool IsSearching { get; }


        public static readonly IProgressInfo Empty = new ProgressInfo();

        public ProgressInfo(int segmentsCompleted, int segments, bool isSearching)
        {
            Completed = segmentsCompleted;
            Total = segments;
            IsSearching = isSearching;
        }

        private ProgressInfo()
        {
            Completed = 1;
            Total = 1;
            IsSearching = false;
        }

        #region Equality

        public bool Equals(ProgressInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Completed == other.Completed && Total == other.Total && IsSearching == other.IsSearching;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProgressInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Completed;
                hashCode = (hashCode*397) ^ Total;
                hashCode = (hashCode*397) ^ IsSearching.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ProgressInfo left, ProgressInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProgressInfo left, ProgressInfo right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}