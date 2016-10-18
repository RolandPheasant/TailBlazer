using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public interface IHasTailInfo
    {
        TailInfo TailInfo { get; }
    }

    public class FileSearchCollection: ILineReader, IHasTailInfo, IEquatable<FileSearchCollection>, IHasLimitationOfLines, IProgressInfo
    {
        public static readonly FileSearchCollection Empty = new FileSearchCollection();
        public long[] Matches { get; }
        public int Count => Matches.Length;
        public int Diff { get; }
        public int Completed { get; }
        public int Total { get; }
        public bool IsSearching { get; }
        public bool HasReachedLimit { get; }
        public int Maximum { get; }
        public TailInfo TailInfo { get; }

        private readonly IDictionary<FileSegmentKey, FileSegmentSearch> _allSearches;

        private FileSegmentSearch LastSearch { get; }

        public IFileMetrics  Metrics { get; }

        private long Size { get; }

        
        public FileSearchCollection(FileSegmentSearch initial,
            TailInfo tailInfo,
            IFileMetrics metrics,
            int limit)
        {
            Metrics = metrics;
            LastSearch = initial;
            _allSearches = new Dictionary<FileSegmentKey, FileSegmentSearch>
            {
                [initial.Key] = initial
            };

            IsSearching = initial.Status != FileSegmentSearchStatus.Complete;
            Total = 1;
            Completed = IsSearching ? 0 : 1;
            Matches = initial.Lines.ToArray();
            TailInfo = tailInfo;
            Size = 0;
            Maximum = limit;
            HasReachedLimit = false;
            Diff = Matches.Length;
        }

        public FileSearchCollection(FileSearchCollection previous, 
            FileSegmentSearch current,
            TailInfo tailInfo,
             IFileMetrics metrics,
            int limit)
        {
            Maximum = limit;
            LastSearch = current;
            Metrics = metrics;
            TailInfo = tailInfo;
            _allSearches = previous._allSearches.Values.ToDictionary(fss => fss.Key);
            _allSearches[current.Key] = current;
            var all = _allSearches.Values.ToArray();

            IsSearching = all.Any(s => s.Segment.Type == FileSegmentType.Head && s.Status != FileSegmentSearchStatus.Complete);
            Total = all.Length;
            Completed = all.Count(s => s.Segment.Type == FileSegmentType.Head && s.Status == FileSegmentSearchStatus.Complete);
            Size = all.Last().Segment.End;

            //For large sets this could be very inefficient
            Matches = all.SelectMany(s => s.Lines).OrderBy(l=>l).ToArray();
            HasReachedLimit = Matches.Length >= limit;
            Diff =  Matches.Length - previous.Matches.Length;
        }


        //public FileSearchCollection(IFileMetrics metrics)
        //{
        //    Matches = new long[0];
        //    HasReachedLimit = false;
        //    TailInfo = TailInfo.Empty;
        //}

        private FileSearchCollection()
        {
            Matches = new long[0];
            HasReachedLimit = false;
            TailInfo = TailInfo.Empty;

            var segment = new FileSegment(FileSegmentType.Head);
            _allSearches = new Dictionary<FileSegmentKey, FileSegmentSearch>
            {
               
                [FileSegmentKey.Tail] = new FileSegmentSearch(segment)
            };
        }

        public bool IsEmpty => this == Empty;

        public IEnumerable<Line> ReadLines(ScrollRequest scroll)
        {
            var page = GetPage(scroll);

            if (page.Size == 0) yield break;

            using (var stream = File.Open(Metrics.FullName, FileMode.Open, FileAccess.Read,FileShare.Delete | FileShare.ReadWrite))
            {

                using (var reader = new StreamReaderExtended(stream, Metrics.Encoding, false))
                {

                    if (page.Size == 0) yield break;

                    foreach (var i in Enumerable.Range(page.Start, page.Size))
                    {
                        if (i > Count - 1) continue;

                        var start = Matches[i];
                        var startPosition = reader.AbsolutePosition();

                        if (startPosition != start)
                        {
                            reader.DiscardBufferedData();
                            reader.BaseStream.Seek(start, SeekOrigin.Begin);
                        }

                         startPosition = reader.AbsolutePosition();

                        var line = reader.ReadLine();
                        var endPosition = reader.AbsolutePosition();
                        var info = new LineInfo(i + 1, i, startPosition, endPosition);

                        yield return new Line(info, line, (DateTime?)null);
                    }
                }
            }
        }

        private Page GetPage(ScrollRequest scroll)
        {
            int first;
            if (scroll.SpecifiedByPosition)
            {
                first = IndexOf(scroll.Position);
            }
            else
            {
                first = scroll.FirstIndex;
            }
          
            int size = scroll.PageSize;

            if (scroll.Mode == ScrollReason.Tail)
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

            return new Page(first, size);
        }

        private int IndexOf(long value)
        {
            for (var i = 0; i < Matches.Length; ++i)
                if (Equals(Matches[i], value))
                    return i;

            return -1;
        }
        

        #region Equality

        public bool Equals(FileSearchCollection other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Matches, other.Matches) && Completed == other.Completed && Total == other.Total && IsSearching == other.IsSearching;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FileSearchCollection) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Matches?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ Completed;
                hashCode = (hashCode*397) ^ Total;
                hashCode = (hashCode*397) ^ IsSearching.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(FileSearchCollection left, FileSearchCollection right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FileSearchCollection left, FileSearchCollection right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return this == Empty ? "<None>" : $"Count: {Count}, Segments: {Total}, Size: {Size}";
        }

    }
}