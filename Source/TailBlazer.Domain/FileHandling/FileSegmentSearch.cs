using System;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    public class FileSegmentSearch : IEquatable<FileSegmentSearch>
    {
        public FileSegmentKey Key { get; }
        public FileSegment Segment { get; }
        public FileSegmentSearchStatus Status { get; }
        public long[] Lines => _matches.Data;

        private readonly ImmutableList<long> _matches;
        

        public FileSegmentSearch(FileSegment segment, FileSegmentSearchStatus status = FileSegmentSearchStatus.Pending)
        {
            Key = segment.Key;
            Segment = segment;
            Status = status;
            _matches = new ImmutableList<long>();
        }

        public FileSegmentSearch(FileSegment segment, FileSegmentSearchResult result)
        {
            Key = segment.Key;
            Segment = segment;
            Status =  FileSegmentSearchStatus.Complete;
            _matches = new ImmutableList<long>(result.Indicies);
        }

        public FileSegmentSearch(FileSegmentSearch segmentSearch, FileSegmentSearchResult result)
        {
            //this can only be the tail as the tail will continue to grow
            Key = segmentSearch.Key;
            Segment = new FileSegment(segmentSearch.Segment, result.End); 
            Status = FileSegmentSearchStatus.Complete;
            _matches = segmentSearch._matches.Add(result.Indicies);
        }


        #region Equality

        public bool Equals(FileSegmentSearch other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Key.Equals(other.Key) && Equals(Segment, other.Segment) && Status == other.Status;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FileSegmentSearch) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Key.GetHashCode();
                hashCode = (hashCode*397) ^ (Segment?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (int) Status;
                return hashCode;
            }
        }

        public static bool operator ==(FileSegmentSearch left, FileSegmentSearch right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FileSegmentSearch left, FileSegmentSearch right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"{Segment} ->{Status}. Items: {Lines.Length}";
        }
    }
}