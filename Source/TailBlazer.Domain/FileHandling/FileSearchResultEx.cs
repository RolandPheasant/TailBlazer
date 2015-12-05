using System;
using System.Collections.Generic;
using System.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public static class FileSearchResultEx
    {

        public static IEnumerable<LineIndex> GetIndicies(this FileSearchResult source, ScrollRequest scroll)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (scroll == null) throw new ArgumentNullException(nameof(scroll));

            int first = scroll.FirstIndex;
            int size = scroll.PageSize;

            if (scroll.Mode == ScrollingMode.Tail)
            {
                first = size > source.Count ? 0 : source.Count - size;
            }
            else
            {
                if (scroll.FirstIndex + size >= source.Count)
                    first = source.Count - size;
            }

            return Enumerable.Range(first, Math.Min(size, source.Count))
                .Select(i =>
                {
                    var start = i == 0 ? 0 : source.Matches[i - 1];
                    var end = source.Matches[i] - 1;
                    return new LineIndex(0, i, start, end);
                });
        }

        public static IEnumerable<LineIndex> GetIndicies(this FileSearchResult source, ScrollRequest scroll, IIndexCollection collection)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (scroll == null) throw new ArgumentNullException(nameof(scroll));

            int first = scroll.FirstIndex;
            int size = scroll.PageSize;

            if (scroll.Mode == ScrollingMode.Tail)
            {
                first = size > source.Count ? 0 : source.Count - size;
            }
            else
            {
                if (scroll.FirstIndex + size >= source.Count)
                    first = source.Count - size;
            }

            first = Math.Max(0, first);
            size = Math.Min(size, source.Count);


          




            //need to look up line number
            return Enumerable.Range(first, Math.Min(size, source.Count))
                .Select(i =>
                {
                    var start = source.Matches[i];
                    var end = i == source.Count - 1 ? source.Size : source.Matches[i + 1];
                    //var start = i == 0 ? 0 : source.Matches[i - 1];
                    //var end = source.Matches[i] - 1;

                    var lineNumber = (int)collection.GetLineNumberFromPosition(start);
                    return new LineIndex(lineNumber, i, start, end);
                });
        }
    }

    //public IEnumerable<LineIndex> GetIndicies(ScrollRequest scroll, LineMatches matches)
    //{
    //    throw new System.NotImplementedException();
    //}
}
