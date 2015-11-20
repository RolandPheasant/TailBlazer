using System.Collections.Generic;
using System.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public  static class LineIndexEx
    {
        public static IEnumerable<LineIndex> GetTail(this LineIndicies source, ScrollRequest scroll)
        {
            var offset = scroll.PageSize > source.Count ? 0 : source.Count - scroll.PageSize;

            return Enumerable.Range(offset, scroll.PageSize)
                .Select(i =>
                {
                    var current = source.Lines[i];
                    var previous = i == 0 ? 0 : source.Lines[i - 1];
                    return new LineIndex(i+1,i,previous,current);

                });
        }

        public static IEnumerable<LineIndex> GetFromIndex(this LineIndicies source, ScrollRequest scroll)
        {

            if (scroll.FirstIndex + scroll.PageSize >= source.Count)
                return source.GetTail(scroll);

            return Enumerable.Range(scroll.FirstIndex, scroll.PageSize)
                .Select(i =>
                {
                    var current = source.Lines[i];
                    var previous = i == 0 ? 0 : source.Lines[i - 1];
                    return new LineIndex(i + 1, i, previous, current);

                });
        }

        public static IEnumerable<LineIndex> GetTail(this LineIndicies source,  ScrollRequest scroll, LineMatches matches)
        {
            var offset = scroll.PageSize > matches.Count ? 0 : matches.Count - scroll.PageSize;

            return matches.Lines
                .Skip(offset)
                .Select((line, index) =>
                {
                    var previous = line == 0 ? 0 : source.Lines[line - 1];
                    var end = source.Lines[line];
                    return new LineIndex(line + 1, index + offset - 1, previous, end);
                }).Take(scroll.PageSize);
        }

        public static IEnumerable<LineIndex> GetFromIndex(this LineIndicies source, ScrollRequest scroll, LineMatches matches)
        {

            if (scroll.FirstIndex + scroll.PageSize > source.Count)
                return source.GetTail(scroll);

            return matches.Lines
                .Skip(scroll.FirstIndex)
                .Select((line, index) =>
                {
                    //TODO: Why is this different from above
                    var previous = line == 0 ? 0 : source.Lines[line - 1];
                    var end = source.Lines[line];
                    return new LineIndex(line + 1, index + scroll.FirstIndex, previous, end);
                }).Take(scroll.PageSize);
        }


    }
}