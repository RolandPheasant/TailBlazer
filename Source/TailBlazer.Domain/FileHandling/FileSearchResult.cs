using System;
using System.Collections.Generic;
using System.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public class FileSearchResult//: ILineProvider
        : IEquatable<FileSearchResult>
    {
        public static readonly FileSearchResult None = new FileSearchResult();
        public long[] Matches { get; }
        public int Count => Matches.Length;
        public int SegmentsCompleted { get; }
        public int Segments { get; }
        public bool IsSearching { get; }

       // private readonly FileSegmentSearch[] _allSearches;
        private readonly IDictionary<FileSegmentKey, FileSegmentSearch> _allSearches;

        public FileSegmentSearch LastSearch { get; }
        public long Size { get; }
        //  public object  =>_lastSearch

        public FileSearchResult(FileSegmentSearch initial)
        {
            LastSearch = initial;
            _allSearches = new Dictionary<FileSegmentKey, FileSegmentSearch>();
            _allSearches[initial.Key] = initial;

            IsSearching = initial.Status != FileSegmentSearchStatus.Complete;
            Segments = 1;
            SegmentsCompleted = IsSearching ? 0 : 1;
            Matches = initial.Lines.ToArray();

            //check this
            Size = 0;

            Console.WriteLine($"{SegmentsCompleted}/{Segments}. {Count}");
        }
        
        public FileSearchResult(FileSearchResult previous, FileSegmentSearch current)
        {
            LastSearch = current;

            _allSearches = previous._allSearches.Values.ToDictionary(fss => fss.Key);
            _allSearches[current.Key] = current;
            var all = _allSearches.Values.ToArray();

            IsSearching = all.Any(s => s.Segment.Type == FileSegmentType.Head && s.Status != FileSegmentSearchStatus.Complete);
            Segments = all.Length;
            SegmentsCompleted = all.Count(s => s.Segment.Type == FileSegmentType.Head && s.Status == FileSegmentSearchStatus.Complete);

            Size = all.Last().Segment.End;

            //For large sets this could be very inefficient
            Matches = all.SelectMany(s => s.Lines).OrderBy(l=>l).ToArray();
            
            Console.WriteLine($"{SegmentsCompleted}/{Segments}. {Count}");
        }
        
        private FileSearchResult()
        {
            Matches = new long[0];
        }

        #region Equality

        public bool Equals(FileSearchResult other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Matches, other.Matches) && SegmentsCompleted == other.SegmentsCompleted && Segments == other.Segments && IsSearching == other.IsSearching;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FileSearchResult) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Matches?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ SegmentsCompleted;
                hashCode = (hashCode*397) ^ Segments;
                hashCode = (hashCode*397) ^ IsSearching.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(FileSearchResult left, FileSearchResult right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FileSearchResult left, FileSearchResult right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return this == None ? "<None>" : $"Count: {Count}, Segments: {Segments}, Size: {Size}";
        }
    }
}