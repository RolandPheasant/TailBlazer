using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public class IndexCollection: IIndexCollection
    {
        public Encoding Encoding { get; }
        public int Count { get; }
        public int Diff { get; }
        public bool IsEmpty => Count != 0; 

        public LinesChangedReason ChangedReason { get; }
        public long TailStartsAt { get; }
        private Index[] Indicies { get; }

        public IndexCollection(IReadOnlyCollection<Index> latest,
                                    IndexCollection previous,
                                    Encoding encoding)
        {

            Encoding = encoding;
            Count = latest.Select(idx => idx.LineCount).Sum();
            Indicies = latest.ToArray();
            Diff = Count - (previous?.Count ?? 0);

            //need to check whether
            if (previous == null)
            {
                ChangedReason = LinesChangedReason.Loaded;
                TailStartsAt = latest.Max(idx => idx.End);
            }
            else
            {
                var mostRecent = latest.OrderByDescending(l => l.TimeStamp).First();
                ChangedReason = mostRecent.Type == IndexType.Tail
                                ? LinesChangedReason.Tailed
                                : LinesChangedReason.Paged;

                TailStartsAt = previous.TailStartsAt;
            }
        }

        public IEnumerable<LineInfo> GetIndicies(ScrollRequest scroll)
        {
            int first = scroll.FirstIndex;
            int size = scroll.PageSize;

            if (scroll.Mode == ScrollingMode.Tail)
            {
                first = size > Count ? 0 : Count - size;
            }
            else
            {
                if (scroll.FirstIndex + size >= Count)
                    first = Count - size;
            }
            
            var relativeIndex = CalculateRelativeIndex(first);
            if (relativeIndex==null) yield break;

            var offset = relativeIndex.LinesOffset;
            foreach (var i in Enumerable.Range(first, Math.Min(size, Count)))
            {
                yield return  new LineInfo(i + 1, i, relativeIndex.Start, offset);
                offset++;
            }
        }

        private RelativeIndex CalculateRelativeIndex(int index)
        {
            int firstLineInContainer = 0;
            int lastLineInContainer = 0;

            foreach (var sparseIndex in Indicies)
            {
                lastLineInContainer += sparseIndex.LineCount;
                if (index < lastLineInContainer)
                {
                    //It could be that the user is scrolling into a part of the file
                    //which is still being indexed [or will never be indexed]. 
                    //In this case we need to estimate where to scroll to
                    if (sparseIndex.LineCount != 0 && sparseIndex.Indicies.Count == 0)
                    {
                        //return estimate here!
                        var lines = sparseIndex.LineCount;
                        var bytes = sparseIndex.End - sparseIndex.Start;
                        var bytesPerLine = bytes/lines;
                        var estimate = index*bytesPerLine;
                        return new RelativeIndex(index, estimate, 0);
                    }

                    var relativePosition = (index - firstLineInContainer);
                    var relativeIndex = relativePosition / sparseIndex.Compression;
                    var offset = relativePosition % sparseIndex.Compression;
                    var start = relativeIndex == 0 ? 0 : sparseIndex.Indicies[relativeIndex - 1];
                    return new RelativeIndex(index, start, offset);
                }
                firstLineInContainer = firstLineInContainer + sparseIndex.LineCount;
            }
            return null;
        }

        private class RelativeIndex
        {
            public int Index { get; }
            public long Start { get; }
            public int LinesOffset { get; }

            public RelativeIndex(int index, long start, int linesOffset)
            {
                Index = index;
                Start = start;
                LinesOffset = linesOffset;
            }
        }
    }
}