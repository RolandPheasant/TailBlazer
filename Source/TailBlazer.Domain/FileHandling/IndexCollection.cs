using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public class IndexCollection: ILineProvider
    {
        public int Count { get; }
        public int Diff { get; }
        public bool IsEmpty => Count != 0;

        private LinesChangedReason ChangedReason { get; }

        private Index[] Indicies { get; }

        private FileInfo Info { get; }
        private Encoding Encoding { get; }
        private TailInfo TailInfo { get; }

        public IndexCollection(IReadOnlyCollection<Index> latest,
                                    IndexCollection previous,
                                    FileInfo info,
                                    Encoding encoding)
        {
            Info = info;
            Encoding = encoding;
            Count = latest.Select(idx => idx.LineCount).Sum();
            Indicies = latest.ToArray();
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
            }
        }

        /// <summary>
        /// Reads the lines.
        /// </summary>
        /// <param name="scroll">The scroll.</param>
        /// <returns></returns>
        public IEnumerable<Line> ReadLines(ScrollRequest scroll)
        {
            if (scroll.SpecifiedByPosition)
            {
                foreach (var line in ReadLinesByPosition(scroll))
                {
                    yield return line;
                }

                yield break;
            }



            var page = GetPage(scroll);

            var relativeIndex = CalculateRelativeIndex(page.Start);
            if (relativeIndex == null) yield break;

            var offset = relativeIndex.LinesOffset;

            using (var stream = File.Open(Info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var reader = new StreamReaderExtended(stream, Encoding, false))
                {
                    //go to starting point
                    stream.Seek(relativeIndex.Start, SeekOrigin.Begin);
                    if (offset > 0)
                    {
                        //skip number of lines offset
                        for (int i = 0; i < offset; i++)
                            reader.ReadLine();
                    }

                    //if estimate move to the next start of line
                    if (relativeIndex.IsEstimate && relativeIndex.Start != 0)
                        reader.ReadLine();

                    foreach (var i in Enumerable.Range(page.Start, page.Size))
                    {
                        var startPosition = reader.AbsolutePosition();
                        var line = reader.ReadLine();
                        var endPosition = reader.AbsolutePosition();
                        var info = new LineInfo(i + 1, i, startPosition, endPosition);

                        var ontail = startPosition >= TailInfo.TailStartsAt && DateTime.Now.Subtract(TailInfo.LastTail).TotalSeconds < 1
                            ? DateTime.Now
                            : (DateTime?)null;

                        yield return new Line(info, line, ontail);
                    }
                }
            }
        }


        private IEnumerable<Line> ReadLinesByPosition(ScrollRequest scroll)
        {

            //TODO: Calculate initial index of first item.

   
            //scroll from specified position

            using (var stream = File.Open(Info.FullName, FileMode.Open, FileAccess.Read,FileShare.Delete | FileShare.ReadWrite))
            {
                int taken = 0;
                using (var reader = new StreamReaderExtended(stream, Encoding, false))
                {

                    var startPosition = scroll.FirstIndex;
                    var first = CalculateIndexByPositon(startPosition);
                    reader.BaseStream.Seek(scroll.FirstIndex, SeekOrigin.Begin);
                    string line;
                    while ((line = reader.ReadLine()) != null && taken < scroll.PageSize)
                    {

                        var endPosition = reader.AbsolutePosition();
                        var info = new LineInfo(taken + 1, taken, startPosition, endPosition);
                        var ontail = endPosition >= TailInfo.TailStartsAt && DateTime.Now.Subtract(TailInfo.LastTail).TotalSeconds < 1
                            ? DateTime.Now
                            : (DateTime?) null;

                        yield return new Line(info, line, ontail);
                        taken++;
                    }
                }
            }
        }

        private Page GetPage(ScrollRequest scroll)
        {
            int first = scroll.FirstIndex;
            int size = scroll.PageSize;


            if (scroll.Mode == ScrollReason.Tail)
            {
                first = size > Count ? 0 : Count - size;
            }
            else
            {

                if (scroll.SpecifiedByPosition)
                {
                    //get line number fro
                    first = this.IndexOf(scroll.FirstIndex);
                }
                else
                {
                    if (scroll.FirstIndex + size >= Count)
                        first = Count - size;
                }

            }

            first = Math.Max(0, first);
            size = Math.Min(size, Count);

            return new Page(first, size);
        }
        private int IndexOf(long value)
        {
            for (var i = 0; i < this.Indicies.Length; ++i)
            {
                if (Equals(Indicies[i], value))
                    return i;

            }

            return -1;
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
                    //It could be that the user is scrolling into a part of the file
                    //which is still being indexed [or will never be indexed]. 
                    //In this case we need to estimate where to scroll to
                    if (sparseIndex.LineCount != 0 && sparseIndex.Indicies.Count == 0)
                    {
                        return -1;
                        //return estimate here!
                        //var lines = sparseIndex.LineCount;
                        //var bytes = sparseIndex.End - sparseIndex.Start;
                        //var bytesPerLine = bytes / lines;
                        //var estimate = index * bytesPerLine;


                        //return new RelativeIndex(index, estimate, 0, true);
                    }
               
                    var relativeIndex = (int)(position / sparseIndex.Compression);
                    var offset = position % sparseIndex.Compression;

                    var actualIndex = sparseIndex.Indicies.IndexOf(position);

                    return actualIndex + offset;
                }
                firstLineInContainer = firstLineInContainer + sparseIndex.LineCount;
            }
            return -1;
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


                        return new RelativeIndex(index, estimate, 0,true);
                    }

                    var relativePosition = (index - firstLineInContainer);
                    var relativeIndex = relativePosition / sparseIndex.Compression;
                    var offset = relativePosition % sparseIndex.Compression;
                    var start = relativeIndex == 0 ? 0 : sparseIndex.Indicies[relativeIndex - 1];
                    return new RelativeIndex(index, start, offset,false);
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
            public bool IsEstimate { get;  }


            public RelativeIndex(int index, long start, int linesOffset, bool isEstimate)
            {
                Index = index;
                Start = start;
                LinesOffset = linesOffset;
                IsEstimate = isEstimate;
            }
        }
    }
}