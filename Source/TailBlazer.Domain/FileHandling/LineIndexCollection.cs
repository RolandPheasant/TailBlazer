using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public class LineIndexCollection : IIndexCollection
    {
        public Encoding Encoding { get;  }
        public long[] Lines { get; }
        public int Count => Lines.Length;
        public int Diff { get; }
        public LinesChangedReason ChangedReason { get; }
        public long TailStartsAt { get; }
        
        public LineIndexCollection(long[] lines, Encoding encoding,  LineIndexCollection previous = null)
        {
            Encoding = encoding;
            if (previous == null)
            {
                Lines = lines;
                Diff = lines.Length;
                ChangedReason = LinesChangedReason.Loaded;
                TailStartsAt = lines.Length - 1;
            }
            else
            {
                //combine the 2 arrays
                var latest = new long[previous.Lines.Length + lines.Length];
                previous.Lines.CopyTo(latest, 0);
                lines.CopyTo(latest, previous.Lines.Length);

                Lines = latest;
                Diff = lines.Length;
                ChangedReason = LinesChangedReason.Tailed;
                TailStartsAt = previous.Count - 1;
            }
        }

        public IEnumerable<LineIndex> GetIndicies(ScrollRequest scroll)
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

            return Enumerable.Range(first, Math.Min(size, Count))
                .Select(i =>
                {
                    var start = i == 0 ? 0 : Lines[i - 1];
                    var end = Lines[i] - 1;
                    return new LineIndex(i + 1, i, start, end);

                });
        }

        public  IEnumerable<LineIndex> GetIndicies(ScrollRequest scroll, LineMatches matches)
        {

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
            foreach (var index in allMatched)
            {
                if (index >= matches.Count) continue;
                var line = matches.Lines[index];

                if (line >= Count - 1) continue;
                var start = line == 0 ? 0 : Lines[line - 1];
                var end = Lines[line] - 1;
                yield return new LineIndex(line, i + first, start, end);
                i++;
            }
        }
    }
}