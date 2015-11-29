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

                if (mostRecent.Type== SpareIndexType.Tail)
                //check which notificaion has changed
                ChangedReason = mostRecent.Type == SpareIndexType.Tail
                                ? LinesChangedReason.Tailed
                                : LinesChangedReason.Paged;

                TailStartsAt = previous.TailStartsAt;

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
            var relativePosition = (element.Element - element.FirstLine);
            var indexInContainer = relativePosition/element.Index.Compression;
            var extraOffset = relativePosition%element.Index.Compression;

            throw new NotImplementedException();
            //return Enumerable.Range(first, Math.Min(size, Count))
            //        .Select(i =>
            //        {
            //            var start = i == 0 ? 0 : Lines[i - 1];
            //            var end = Lines[i] - 1;
            //            return new LineIndex(i + 1, i, start, end);

            //        });
        }

        private Optional<IndexedContainer> FindPage(int i)
        {
            int firstLineInContainer = 0;
            foreach (var sparseIndex in Indicies)
            {
               
               // accumulatedCount
                if (i < sparseIndex.LineCount)
                    return new IndexedContainer(i, sparseIndex, firstLineInContainer);;

                firstLineInContainer = firstLineInContainer + sparseIndex.LineCount;
            }
            return null;
        }

        private class IndexedContainer
        {
            public int Element { get; }
            public SparseIndex Index { get; }
            public int FirstLine { get; }

            public IndexedContainer(int element, SparseIndex index, int firstLine)
            {
                Element = element;
                Index = index;
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

            throw new NotImplementedException();
            //foreach (var index in allMatched)
            //{
            //    if (index >= matches.Count) continue;
            //    var line = matches.Lines[index];

            //    if (line >= Count - 1) continue;
            //    var start = line == 0 ? 0 : Lines[line - 1];
            //    var end = Lines[line] - 1;
            //    yield return new LineIndex(line, i + first, start, end);
            //    i++;
            //}
        }
    }
}