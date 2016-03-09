using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DynamicData.Kernel;

namespace TailBlazer.Domain.FileHandling
{
    public interface IProgressInfo
    {
        int SegmentsCompleted { get; }
        int Segments { get; }
        bool IsSearching { get; }
    }

    public class FileSearchResult: ILineProvider, IEquatable<FileSearchResult>, IHasLimitationOfLines, IProgressInfo
    {
        public static readonly FileSearchResult None = new FileSearchResult();
        public long[] Matches { get; }
        public int Count => Matches.Length;
        public int SegmentsCompleted { get; }
        public int Segments { get; }
        public bool IsSearching { get; }
        public bool HasReachedLimit { get; }
        public int Maximum { get; }

        private readonly IDictionary<FileSegmentKey, FileSegmentSearch> _allSearches;

        private FileSegmentSearch LastSearch { get; }
        private FileInfo Info { get;  }
        private Encoding Encoding { get;  }
        private TailInfo TailInfo { get; }
        private long Size { get; }

        public FileSearchResult(FileSegmentSearch initial,
            FileInfo info,
            Encoding encoding,
            int limit)
        {
            Info = info;
            Encoding = encoding;
            LastSearch = initial;
            _allSearches = new Dictionary<FileSegmentKey, FileSegmentSearch>
            {
                [initial.Key] = initial
            };

            IsSearching = initial.Status != FileSegmentSearchStatus.Complete;
            Segments = 1;
            SegmentsCompleted = IsSearching ? 0 : 1;
            Matches = initial.Lines.ToArray();
            TailInfo = TailInfo.None;
            Size = 0;
            Maximum = limit;
            HasReachedLimit = false;
        }

        public FileSearchResult(FileSearchResult previous, 
            FileSegmentSearch current,
            FileInfo info,
            Encoding encoding,
            int limit)
        {
            Maximum = limit;
            LastSearch = current;
            Info = info;
            Encoding = encoding;

            _allSearches = previous._allSearches.Values.ToDictionary(fss => fss.Key);

            var lastTail = _allSearches.Lookup(FileSegmentKey.Tail);
            if (current.Segment.Type == FileSegmentType.Tail)
            {
                TailInfo = lastTail.HasValue 
                            ? new TailInfo(lastTail.Value.Segment.End) 
                            : new TailInfo(current.Segment.End);
            }
            else
            {
                TailInfo = lastTail.HasValue 
                            ? previous.TailInfo 
                            : TailInfo.None;
            }
            
            _allSearches[current.Key] = current;
            var all = _allSearches.Values.ToArray();

            IsSearching = all.Any(s => s.Segment.Type == FileSegmentType.Head && s.Status != FileSegmentSearchStatus.Complete);
            Segments = all.Length;
            SegmentsCompleted = all.Count(s => s.Segment.Type == FileSegmentType.Head && s.Status == FileSegmentSearchStatus.Complete);
            Size = all.Last().Segment.End;

            //For large sets this could be very inefficient
            Matches = all.SelectMany(s => s.Lines).OrderBy(l=>l).ToArray();
            HasReachedLimit = Matches.Length >= limit;
        }

        private FileSearchResult()
        {
            Matches = new long[0];
            HasReachedLimit = false;
        }

        public bool IsEmpty => this == None;

        public IEnumerable<Line> ReadLines(ScrollRequest scroll)
        {


            var page = GetPage(scroll);

            if (page.Size == 0) yield break;

            using (var stream = File.Open(Info.FullName, FileMode.Open, FileAccess.Read,FileShare.Delete | FileShare.ReadWrite))
            {

                using (var reader = new StreamReaderExtended(stream, Encoding, false))
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
                        
                        var ontail = endPosition >= TailInfo.TailStartsAt && DateTime.Now.Subtract(TailInfo.LastTail).TotalSeconds<1
                                    ? DateTime.Now 
                                    : (DateTime?)null; 

                        yield return new Line(info, line, ontail);
                    }
                }
            }
        }


        private Page GetPage(ScrollRequest scroll)
        {
            int first;
            if (scroll.SpecifiedByPosition)
            {
                //get line number fro
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
            if (obj.GetType() != GetType()) return false;
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