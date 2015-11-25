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
                    var current = source.Lines[i] - 1;
                    var previous = i == 0 ? 0 : source.Lines[i - 1];
                    return new LineIndex(i + 1, i, previous, current);

                });
        }

        public static IEnumerable<LineIndex> GetTail(this LineIndicies source,  ScrollRequest scroll, LineMatches matches)
        {
            //the indexer and matcher can be out of phase so take most recent matches

            int counter = 0;
            foreach (var line in matches.Lines.Reverse())
            {
                if (line > source.Count -1 )
                    continue;

                counter++;
                var previous = line == 0 ? 0 : source.Lines[line - 1];
                var end = source.Lines[line];

                yield return new LineIndex(line + 1, matches.Count - counter - 1, previous, end);

                if (counter== scroll.PageSize)
                    yield break;;
            }
        }

        public static IEnumerable<LineIndex> GetIndicies(this LineIndicies source, ScrollRequest scroll, LineMatches matches)
        {

            int first = scroll.FirstIndex;
            int size = scroll.PageSize;


            if (scroll.Mode == ScrollingMode.Tail)
            {
                first = size > matches.Count ? 0 : matches.Count - size;
            }
            else
            {
                if (first + size >= source.Count)
                    first = matches.Count - size;
            }

            var allMatched = Enumerable.Range(first, Math.Min(size,matches.Count))
                        .Select(index=> matches.Lines[index]).ToArray();


            int i = 0;
            foreach (var line in allMatched)
            {
                var start = line == 0 ? 0 : source.Lines[line - 1];
                var end = source.Lines[line]-1;
                yield return new LineIndex(line, i + first, start, end);
                i++;
            }
        }



    }
}