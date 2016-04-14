using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DynamicData;
using DynamicData.Kernel;

namespace TailBlazer.Domain.FileHandling
{
    public interface IProgressInfo
    {
        int SegmentsCompleted { get; }
        int Segments { get; }
        bool IsSearching { get; }
    }

    public class FileSearchResult : ILineProvider, IEquatable<FileSearchResult>, IHasLimitationOfLines, IProgressInfo
    {
        public static readonly FileSearchResult None = new FileSearchResult();
        private readonly IDictionary<FileSegmentKey, FileSegmentSearch> _allSearches;

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

            IsSearching =
                all.Any(s => s.Segment.Type == FileSegmentType.Head && s.Status != FileSegmentSearchStatus.Complete);
            Segments = all.Length;
            SegmentsCompleted =
                all.Count(s => s.Segment.Type == FileSegmentType.Head && s.Status == FileSegmentSearchStatus.Complete);
            Size = all.Last().Segment.End;

            //For large sets this could be very inefficient
            Matches = all.SelectMany(s => s.Lines).OrderBy(l => l).ToArray();
            HasReachedLimit = Matches.Length >= limit;
            
            //skip the same file in here, because we do not want to store in the list.
            if (previous.Info.FullName != info.FullName)
            {
                var prevPointer = previous;
                //seek to the end of the linked list
                while (prevPointer?.Next != null)
                {
                    prevPointer = prevPointer.Next as FileSearchResult;
                }
                var lastPrevPointer = default(FileSearchResult);
                //find the modified file and replace from the list
                while (prevPointer != null)
                {
                    if (prevPointer.Info.FullName == Info.FullName && prevPointer.Count != Count)
                    {
                        Next = prevPointer.Next;
                        Previous = prevPointer.Previous;
                        if (lastPrevPointer != null)
                        {
                            lastPrevPointer.Previous = this;
                        }
                        prevPointer.Previous = null;
                        prevPointer.Next = null;
                        NumberOfPreviousProvider = prevPointer.NumberOfPreviousProvider;
                        return;
                    }
                    lastPrevPointer = prevPointer;
                    prevPointer = prevPointer.Previous as FileSearchResult;
                }

                //build the linked list in here
                Previous = previous;
                previous.Next = this;
                NumberOfPreviousProvider = previous.NumberOfPreviousProvider + 1;
            }

        }

        private FileSearchResult()
        {
            Matches = new long[0];
            HasReachedLimit = false;
        }

        public long[] Matches { get; }
        private FileSegmentSearch LastSearch { get; }
        private FileInfo Info { get; }
        private Encoding Encoding { get; }
        private TailInfo TailInfo { get; }
        private long Size { get; }
        public bool IsEmpty => this == None;
        public bool HasReachedLimit { get; }
        public int Maximum { get; }
        public int Count => Matches.Length;
        public ILineProvider Previous { get; set; }
        public ILineProvider Next { get; set; }
        public int NumberOfPreviousProvider { get; }

        public IEnumerable<Line> ReadLines(ScrollRequest scroll)
        {
            var current = ReverseLinking(this);
            var page = GetPage(scroll, this);
            var lastValueWrapper = new LastValueWrapper();
            if (page.Size == 0) yield break;
            while (current != null)
            {
                using (
                    var stream = File.Open(current.Info.FullName, FileMode.Open, FileAccess.Read,
                        FileShare.Delete | FileShare.ReadWrite))
                {
                    using (var reader = new StreamReaderExtended(stream, current.Encoding, false))
                    {
                        if (page.Size == 0) yield break;

                        if (lastValueWrapper.LastPageIndex == page.Start + page.Size)
                        {
                            yield break;
                        }
                        var counter = 0;
                        long lastEndPos = 0;
                        foreach (
                            var i in
                                Enumerable.Range(
                                    lastValueWrapper.LastPageIndex > 0 ? lastValueWrapper.LastPageIndex : page.Start,
                                    page.Size - counter))
                        {
                            if (i == page.Start + page.Size)
                            {
                                yield break;
                            }
                            if (i - lastValueWrapper.LastMatchesSize > current.Count - 1)
                            {
                                lastValueWrapper.LastEndPosition += lastEndPos + 1;
                                lastValueWrapper.LastMatchesSize += current.Count;
                                break;
                            }

                            var start = current.Matches[i - lastValueWrapper.LastMatchesSize];
                            var startPosition = reader.AbsolutePosition();

                            if (startPosition != start)
                            {
                                reader.DiscardBufferedData();
                                reader.BaseStream.Seek(start, SeekOrigin.Begin);
                            }

                            startPosition = reader.AbsolutePosition();

                            var line = reader.ReadLine();
                            var endPosition = reader.AbsolutePosition();
                            var info = new LineInfo(i + 1, i, startPosition + lastValueWrapper.LastEndPosition,
                                endPosition + lastValueWrapper.LastEndPosition);

                            var ontail = endPosition >= TailInfo.TailStartsAt &&
                                         DateTime.Now.Subtract(TailInfo.LastTail).TotalSeconds < 1
                                ? DateTime.Now
                                : (DateTime?) null;

                            yield return new Line(info, line, ontail);

                            lastValueWrapper.LastPageIndex = i + 1;
                            counter++;
                            lastEndPos = endPosition;

                            if (reader.EndOfStream)
                            {
                                lastValueWrapper.LastEndPosition += endPosition + 1;
                                lastValueWrapper.LastMatchesSize += current.Count;
                                break;
                            }
                        }
                    }
                }
                current = current.Next as FileSearchResult;
            }
        }

        private FileSearchResult ReverseLinking(FileSearchResult fsr)
        {
            //reverse link of list
            var current = fsr;
            while (current?.NumberOfPreviousProvider != 0)
            {
                if (current?.Previous != null)
                {
                    current.Previous.Next = current;
                }
                current = current?.Previous as FileSearchResult;
            }
            return current;
        }

        public int SegmentsCompleted { get; }
        public int Segments { get; }
        public bool IsSearching { get; }

        private Page GetPage(ScrollRequest scroll, FileSearchResult fileSearchResult)
        {
            var first = scroll.SpecifiedByPosition ? IndexOf(scroll.Position) : scroll.FirstIndex;
            var chainSize = 0;
            while (fileSearchResult != null)
            {
                chainSize += fileSearchResult.Count;
                fileSearchResult = fileSearchResult.Previous as FileSearchResult;
            }

            var size = scroll.PageSize;

            if (scroll.Mode == ScrollReason.Tail)
            {
                first = size > chainSize ? 0 : chainSize - size;
            }
            else
            {
                if (scroll.FirstIndex + size >= chainSize)
                    first = chainSize - size;
            }

            first = Math.Max(0, first);
            size = Math.Min(size, chainSize);

            return new Page(first, size);
        }

        private int IndexOf(long value)
        {
            for (var i = 0; i < Matches.Length; ++i)
                if (Equals(Matches[i], value))
                    return i;

            return -1;
        }

        public override string ToString()
        {
            return this == None
                ? "<None>"
                : $"Count: {Count}, Segments: {Segments}, Size: {Size}, Previous: {NumberOfPreviousProvider}";
        }

        #region Equality

        public bool Equals(FileSearchResult other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Matches, other.Matches) && SegmentsCompleted == other.SegmentsCompleted &&
                   Segments == other.Segments && IsSearching == other.IsSearching;
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
    }
}