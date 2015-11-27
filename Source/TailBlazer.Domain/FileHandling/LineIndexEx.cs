using System;
using System.Collections.Generic;
using System.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public  static class LineIndexEx
    {
        public static IEnumerable<LineIndex> GetIndicies(this LineIndicies source, ScrollRequest scroll)
        {
            int first = scroll.FirstIndex;
            int size = scroll.PageSize;

            if (scroll.Mode == ScrollingMode.Tail )
            {
                 first = size > source.Count ? 0 : source.Count - size;
            }
            else
            {
                if (scroll.FirstIndex + size >= source.Count)
                    first = source.Count - size;
            }
            return source.GetIndicies(first, size);
        }

        private static IEnumerable<LineIndex> GetIndicies(this LineIndicies source, int firstIndex, int pageSize)
        {

            return Enumerable.Range(firstIndex, Math.Min(pageSize,source.Count))
                .Select(i =>
                {
                    var start = i == 0 ? 0 : source.Lines[i - 1];
                    var end = source.Lines[i] - 1;
                    return new LineIndex(i + 1, i, start, end);

                });
        }


        public static IEnumerable<LineIndex> GetIndicies(this LineIndicies source, ScrollRequest scroll, LineMatches matches)
        {

            int first = scroll.FirstIndex;
            int size = scroll.PageSize;

            //TODO: If page size increases, it may be that we should fill the page up if current index is after last item 

            if (scroll.Mode == ScrollingMode.Tail)
            {
                first = size > matches.Count ? 0 : matches.Count - size;

            }
            else
            {

                if (first + size >= source.Count)
                    first = matches.Count - size;
            }

            var allMatched = Enumerable.Range(first, Math.Min(size,matches.Count));

            int i = 0;
            foreach (var index in allMatched)
            {
                if (index >= matches.Count) continue;
                var line = matches.Lines[index];

                if (line >= source.Count-1) continue;
                var start = line == 0 ? 0 : source.Lines[line - 1];
                var end = source.Lines[line]-1;
                yield return new LineIndex(line, i + first, start, end);
                i++;
            }
        }



    }
}