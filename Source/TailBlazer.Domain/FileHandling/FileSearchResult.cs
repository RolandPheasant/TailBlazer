using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData.Kernel;

namespace TailBlazer.Domain.FileHandling
{
    public class FileSearchResult: ILineProvider, IEquatable<FileSearchResult>
    {
        public static readonly FileSearchResult None = new FileSearchResult();
        public long[] Matches { get; }
        public int Count => Matches.Length;
        public int SegmentsCompleted { get; }
        public int Segments { get; }
        public bool IsSearching { get; }

        public LinesChangedReason ChangedReason { get; }
        public long TailStartsAt { get; }

        // private readonly FileSegmentSearch[] _allSearches;
        private readonly IDictionary<FileSegmentKey, FileSegmentSearch> _allSearches;

        public FileSegmentSearch LastSearch { get; }

        

        public long Size { get; }

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

            var lastTail = _allSearches.Lookup(FileSegmentKey.Tail);

            if (lastTail.HasValue)
            {
                TailStartsAt = lastTail.Value.Segment.End;
            }
            else
            {
                 TailStartsAt = long.MaxValue;
            }

            _allSearches[current.Key] = current;
            var all = _allSearches.Values.ToArray();

            IsSearching = all.Any(s => s.Segment.Type == FileSegmentType.Head && s.Status != FileSegmentSearchStatus.Complete);
            Segments = all.Length;
            SegmentsCompleted = all.Count(s => s.Segment.Type == FileSegmentType.Head && s.Status == FileSegmentSearchStatus.Complete);
            Size = all.Last().Segment.End;

            //For large sets this could be very inefficient
            Matches = all.SelectMany(s => s.Lines).OrderBy(l=>l).ToArray();

            Console.WriteLine($"{SegmentsCompleted}/{Segments}.{Count}");
        }
        
        private FileSearchResult()
        {
            Matches = new long[0];
        }

        public bool IsEmpty => this == None;

        public  IEnumerable<LineInfo> GetIndicies( ScrollRequest scroll)
        {
            if (scroll == null) throw new ArgumentNullException(nameof(scroll));

            int first = scroll.FirstIndex;
            int size = scroll.PageSize;

            Console.WriteLine("{}");

            if (scroll.Mode == ScrollingMode.Tail)
            {
                first = size > Count ? 0 : Count - size;
            }
            else
            {
                if (scroll.FirstIndex + size >= Count)
                    first = Count - size;
            }

            first = Math.Max(0, first);
            size = Math.Min(size, Count);
            if (size == 0) yield break;

            Console.WriteLine($"ACTUAL SCROLL={scroll.Mode}, {first}, {size}. Number of matches ={Matches.Length}");

            foreach (var i in Enumerable.Range(first, size))
            {
                if (i > Count - 1) continue;

                var start = Matches[i];

                //Console.WriteLine(start);
                yield return new LineInfo(0, i, start, (long)0, start >=TailStartsAt
                    && ChangedReason == LinesChangedReason.Tailed);
            }

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