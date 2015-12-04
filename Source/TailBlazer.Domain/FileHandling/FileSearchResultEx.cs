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
                first = size > source.Total ? 0 : source.Total - size;
            }
            else
            {
                if (scroll.FirstIndex + size >= source.Total)
                    first = source.Total - size;
            }

            return Enumerable.Range(first, Math.Min(size, source.Total))
                .Select(i =>
                {
                    var start = i == 0 ? 0 : source.Matches[i - 1];
                    var end = source.Matches[i] - 1;
                    return new LineIndex(i + 1, i, start, end);
                });
        }
    }

    //public IEnumerable<LineIndex> GetIndicies(ScrollRequest scroll, LineMatches matches)
    //{
    //    throw new System.NotImplementedException();
    //}
}
