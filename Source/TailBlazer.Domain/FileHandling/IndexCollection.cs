using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public class IndexCollection : ILineProvider
    {
        public IndexCollection(IReadOnlyCollection<Index> latest,
            IndexCollection previous,
            FileInfo info,
            Encoding encoding)
        {
            Info = info;
            Encoding = encoding;
            Count = latest.Select(idx => idx.LineCount).Sum();
            Indicies = latest/*.GroupBy(c => c.LineCount).Select(g => g.FirstOrDefault())*/.ToArray();
            Diff = Count - (previous?.Count ?? 0);

            //need to check whether
            if (previous == null)
            {
                ChangedReason = LinesChangedReason.Loaded;
                TailInfo = new TailInfo(latest.Max(idx => idx.End));
            }
            else
            {
                var mostRecent = latest.OrderByDescending(l => l.TimeStamp).First();
                ChangedReason = mostRecent.Type == IndexType.Tail
                    ? LinesChangedReason.Tailed
                    : LinesChangedReason.Paged;

                TailInfo = new TailInfo(previous.Indicies.Max(idx => idx.End));

                Previous = previous;
                previous.Next = this;
                NumberOfPreviousProvider = previous.NumberOfPreviousProvider + 1;

                //var prevPointer = previous;
                ////find the modified file and replace from the list
                //while (prevPointer != null)
                //{
                //    var lastPrevPointer = prevPointer;
                //    if (prevPointer.Info.FullName == Info.FullName)
                //    {
                //        Next = prevPointer.Next;
                //        Previous = prevPointer.Previous;
                //        lastPrevPointer.Previous = this;
                //        prevPointer.Previous = null;
                //        prevPointer.Next = null;
                //        NumberOfPreviousProvider = prevPointer.NumberOfPreviousProvider;
                //        return;
                //    }
                //    prevPointer = prevPointer.Previous as IndexCollection;
                //}
            }
        }

        public IndexCollection(IndexCollection indexCollection)
        {
            Info = indexCollection.Info;
            Encoding = indexCollection.Encoding;
            Count = indexCollection.Count;
            Indicies = indexCollection.Indicies;
            Diff = indexCollection.Diff;
            ChangedReason = indexCollection.ChangedReason;
            TailInfo = indexCollection.TailInfo;
            Previous = indexCollection.Previous;
            NumberOfPreviousProvider = indexCollection.NumberOfPreviousProvider;
        }

        public int Diff { get; }
        public bool IsEmpty => Count != 0;
        private LinesChangedReason ChangedReason { get; }
        public Index[] Indicies { get; }
        public FileInfo Info { get; }
        private Encoding Encoding { get; }
        public TailInfo TailInfo { get; }
        public ILineProvider Previous { get; set; }
        public ILineProvider Next { get; set; }
        public int NumberOfPreviousProvider { get; }
        public int Count { get; }

        /// <summary>
        ///     Reads the lines.
        /// </summary>
        /// <param name="scroll">The scroll.</param>
        /// <returns></returns>
        public IEnumerable<Line> ReadLines(ScrollRequest scroll)
        {
            if (scroll.SpecifiedByPosition)
            {
                foreach (var line in ReadLinesByPosition(scroll))
                    yield return line;
            }
            else
            {
                foreach (var line in ReadLinesByIndex(scroll))
                    yield return line;
            }
        }

        private IEnumerable<Line> ReadLinesByIndex(ScrollRequest scroll)
        {
            var current = this;
            var lastValueWrapper = new LastValueWrapper();
            var iterationCounter = 0;
            var page = GetPage(scroll, current);
            var relativeIndex = CalculateRelativeIndex(page.Start, ref current, lastValueWrapper);

            while (relativeIndex != null && current != null)
            {
                if (current.Indicies.Length > 0 && current.Indicies.Any(t => t.Indicies.Count == 0))
                {
                    iterationCounter++;
                    current = current.Previous as IndexCollection;
                    continue;
                }
                if (lastValueWrapper.LastPageIndex == page.Start + page.Size)
                {
                    yield break;
                }
                var offset = relativeIndex.LinesOffset;
                using (
                    var stream = File.Open(current.Info.FullName, FileMode.Open, FileAccess.Read,
                        FileShare.Delete | FileShare.ReadWrite))
                {
                    using (var reader = new StreamReaderExtended(stream, current.Encoding, false))
                    {
                        //go to starting point
                        stream.Seek((iterationCounter > 0) ? 0 : relativeIndex.Start, SeekOrigin.Begin);
                        if (iterationCounter == 0 && offset > 0)
                        {
                            //skip number of lines offset
                            for (var i = 0; i < offset; i++)
                            {
                                reader.ReadLine();
                            }
                        }

                        //if estimate move to the next start of line
                        if (iterationCounter == 0 && relativeIndex.IsEstimate && relativeIndex.Start != 0)
                        {
                            reader.ReadLine();
                        }

                        foreach (
                            var i in
                                Enumerable.Range((iterationCounter > 0) ? lastValueWrapper.LastPageIndex : page.Start,
                                    page.Size))
                        {
                            if (i == page.Start + page.Size)
                            {
                                yield break;
                            }
                            var startPosition = reader.AbsolutePosition() + lastValueWrapper.LastEndPosition;
                            var line = reader.ReadLine();
                            var endPosition = reader.AbsolutePosition() + lastValueWrapper.LastEndPosition;

                            var info = new LineInfo(i + 1, i, startPosition, endPosition);

                            var ontail = startPosition >= current.TailInfo.TailStartsAt &&
                                         DateTime.Now.Subtract(current.TailInfo.LastTail).TotalSeconds < 1
                                ? DateTime.Now
                                : (DateTime?)null;

                            yield return new Line(info, line, ontail);

                            lastValueWrapper.LastPageIndex = i + 1;

                            if (reader.EndOfStream)
                            {
                                lastValueWrapper.LastEndPosition += endPosition + 1;
                                break;
                            }
                        }
                    }
                }
                iterationCounter++;
                current = current.Previous as IndexCollection;
            }
        }

        private IEnumerable<Line> ReadLinesByPosition(ScrollRequest scroll)
        {
            //TODO: Calculate initial index of first item.


            //scroll from specified position

            using (
                var stream = File.Open(Info.FullName, FileMode.Open, FileAccess.Read,
                    FileShare.Delete | FileShare.ReadWrite))
            {
                var taken = 0;
                using (var reader = new StreamReaderExtended(stream, Encoding, false))
                {
                    var startPosition = scroll.Position;
                    var first = (int)CalculateIndexByPositon(startPosition);
                    reader.BaseStream.Seek(startPosition, SeekOrigin.Begin);

                    do
                    {
                        var line = reader.ReadLine();
                        if (line == null) yield break;

                        var endPosition = reader.AbsolutePosition();

                        var info = new LineInfo(first + taken + 1, first + taken, startPosition, endPosition);
                        var ontail = endPosition >= TailInfo.TailStartsAt &&
                                     DateTime.Now.Subtract(TailInfo.LastTail).TotalSeconds < 1
                            ? DateTime.Now
                            : (DateTime?)null;

                        yield return new Line(info, line, ontail);

                        startPosition = endPosition;
                        taken++;
                    } while (taken < scroll.PageSize);
                }
            }
        }

        private Page GetPage(ScrollRequest scroll, IndexCollection indexCollection)
        {
            var first = scroll.FirstIndex;
            var size = scroll.PageSize;
            var indexCollectionsCount = 0;
            //collect files line count
            while (indexCollection != null)
            {
                if (indexCollection.Indicies.Length > 0 && indexCollection.Indicies.Any(t => t.Indicies.Count == 0))
                {
                    indexCollection = indexCollection.Previous as IndexCollection;
                    continue;
                }
                indexCollectionsCount += indexCollection.Count;
                indexCollection = indexCollection.Previous as IndexCollection;
            }

            if (scroll.Mode == ScrollReason.Tail)
            {
                first = size > indexCollectionsCount ? 0 : indexCollectionsCount - size;
            }
            else if (scroll.FirstIndex + size >= indexCollectionsCount)
            {
                first = indexCollectionsCount - size;
            }

            first = Math.Max(0, first);
            size = Math.Min(size, indexCollectionsCount);

            return new Page(first, size);
        }

        private long CalculateIndexByPositon(long position)
        {
            long firstLineInContainer = 0;
            long lastLineInContainer = 0;

            foreach (var sparseIndex in Indicies)
            {
                lastLineInContainer += sparseIndex.End;
                if (position < lastLineInContainer)
                {
                    if (sparseIndex.LineCount != 0 && sparseIndex.Indicies.Count == 0)
                    {
                        var lines = sparseIndex.LineCount;
                        var bytes = sparseIndex.End - sparseIndex.Start;
                        var bytesPerLine = bytes / lines;

                        return position / bytesPerLine;
                    }


                    if (sparseIndex.Compression == 1)
                    {
                        return firstLineInContainer + sparseIndex.Indicies.IndexOf(position);
                    }

                    //find nearest, then work out offset
                    var nearest = sparseIndex.Indicies.Data
                        .Select((value, index) => new { value, index })
                        .OrderByDescending(x => x.value)
                        .FirstOrDefault(i => i.value <= position);

                    if (nearest != null)
                    {
                        //index depends of how far in container
                        var relativeIndex = nearest.index * sparseIndex.Compression;

                        //remaining size
                        var size = (sparseIndex.End - sparseIndex.Start);
                        var offset = (position - nearest.value);
                        var estimateOffset = (offset / size) * sparseIndex.Compression;
                        return firstLineInContainer + relativeIndex + estimateOffset;
                    }
                    else
                    {
                        //index depends of how far in container
                        var relativeIndex = 0;

                        //remaining size
                        var size = (sparseIndex.End - sparseIndex.Start);
                        var offset = position;
                        var estimateOffset = (offset / size) * sparseIndex.Compression;
                        return firstLineInContainer + relativeIndex + estimateOffset;
                    }
                }
                firstLineInContainer = firstLineInContainer + sparseIndex.LineCount;
            }
            return -1;
        }

        private RelativeIndex CalculateRelativeIndex(int index, ref IndexCollection indexCollection,
            LastValueWrapper lastValueWrapper)
        {
            var firstLineInContainer = 0;
            var lastLineInContainer = 0;

            while (indexCollection != null)
            {
                foreach (var sparseIndex in indexCollection.Indicies)
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
                            var bytesPerLine = bytes / lines;
                            var estimate = index * bytesPerLine;


                            return new RelativeIndex(index, estimate, 0, true);
                        }

                        var relativePosition = (index - firstLineInContainer);
                        var relativeIndex = relativePosition / sparseIndex.Compression;
                        var offset = relativePosition % sparseIndex.Compression;

                        if (relativeIndex >= sparseIndex.IndexCount)
                            relativeIndex = sparseIndex.IndexCount - 1;
                        var start = relativeIndex == 0 ? 0 : sparseIndex.Indicies[relativeIndex - 1];
                        return new RelativeIndex(index, start, offset, false);
                    }
                    firstLineInContainer = firstLineInContainer + sparseIndex.LineCount;
                }
                lastValueWrapper.LastEndPosition += indexCollection.Indicies
                    .Where(localIndex => localIndex.IndexCount > 0)
                    .Sum(localIndex => localIndex.Indicies[localIndex.IndexCount - 1]);

                indexCollection = indexCollection.Previous as IndexCollection;
            }

            return null;
        }

        public override string ToString()
        {
            return TailInfo == TailInfo.None
                ? "<None>"
                : $"Count: {Count}, Indicies: {Indicies.Length}, Diff: {Diff}, Previous: {NumberOfPreviousProvider}";
        }

        private class RelativeIndex
        {
            public RelativeIndex(long index, long start, int linesOffset, bool isEstimate)
            {
                Index = index;
                Start = start;
                LinesOffset = linesOffset;
                IsEstimate = isEstimate;
            }

            public long Index { get; }
            public long Start { get; }
            public int LinesOffset { get; }
            public bool IsEstimate { get; }
        }
    }
}