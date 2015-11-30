using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DynamicData.Kernel;

namespace TailBlazer.Domain.FileHandling
{
    public class SparseIndicies: IIndexCollection
    {
        public Encoding Encoding { get; }
        public int Count { get; }
        public int Diff { get; }
        public LinesChangedReason ChangedReason { get; }
        public int TailStartsAt { get; }

        public SparseIndex[] Indicies { get; }

        public SparseIndicies(IReadOnlyCollection<SparseIndex> latest,
                                    SparseIndicies previous,
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
                TailStartsAt = Count-1;
            }
            else
            {
                var mostRecent = latest.OrderByDescending(l => l.TimeStamp).First();
                ChangedReason = mostRecent.Type == SpareIndexType.Tail
                                ? LinesChangedReason.Tailed
                                : LinesChangedReason.Paged;

                TailStartsAt = previous.Count-1;

            }
        }

        public  IEnumerable<LineIndex> GetIndicies(ScrollRequest scroll)
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

            //To find index
            //1. Find first container / Then can calculate how many lines to skip and how many lines to take
            var page = FindPage(first);
            if (!page.HasValue) yield break;

            var element = page.Value;
            var relativePosition = (element.Index - element.FirstLine);
            var index = relativePosition/element.SparseIndex.Compression;
            var offset = relativePosition%element.SparseIndex.Compression;
            var start = index==0 ? 0 : element.SparseIndex.Indicies[index-1];


            foreach (var i in Enumerable.Range(first, Math.Min(size, Count)))
            {
                yield return  new LineIndex(i + 1, i, start, offset);
                offset++;
            }
        }

        private Optional<IndexedContainer> FindPage(int i)
        {
            int firstLineInContainer = 0;
            int lastLineInContainer = 0;

            foreach (var sparseIndex in Indicies)
            {
                lastLineInContainer += sparseIndex.LineCount;
               // accumulatedCount
                if (i < lastLineInContainer)
                    return new IndexedContainer(i, sparseIndex, firstLineInContainer);;

                firstLineInContainer = firstLineInContainer + sparseIndex.LineCount;
            }
            return null;
        }

        private class IndexedContainer
        {
            public int Index { get; }
            public SparseIndex SparseIndex { get; }
            public int FirstLine { get; }

            public IndexedContainer(int index, SparseIndex sparseIndex, int firstLine)
            {
                Index = index;
                SparseIndex = sparseIndex;
                FirstLine = firstLine;
            }
        }

        public IEnumerable<LineIndex> GetIndicies(ScrollRequest scroll, LineMatches matches)
        {
            //abstract clipping
            int first = scroll.FirstIndex;
            int size = scroll.PageSize;

            if (scroll.Mode == ScrollingMode.Tail)
            {
                first = size > matches.Count ? 0 : matches.Count - size;

            }
            else
            {
                if (first + size >= matches.Count)
                    first = matches.Count - size;
            }

            var allMatched = Enumerable.Range(Math.Max(first, 0), Math.Min(size, matches.Count));

            int i = 0;


            foreach (var item in allMatched)
            {
                if (item >= matches.Count) continue;
                var line = matches.Lines[item];

                if (line>=Count-1) continue;

                var page = FindPage(line);
                if (!page.HasValue) yield break;

                var element = page.Value;
                var relativePosition = (element.Index - element.FirstLine);
                var index = relativePosition / element.SparseIndex.Compression;

                var offset = relativePosition % element.SparseIndex.Compression;
                var start = index == 0 ? 0 : element.SparseIndex.Indicies[index - 1];

                yield return new LineIndex(line, i + first, start, offset);
                i++;
            }
        }
    }
}