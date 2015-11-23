using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public class LineIndicies
    {
        public Encoding Encoding { get;  }
        public int LineFeedSize { get;  }

        public int[] Lines { get; }
        public int Count => Lines.Length;
        public int Diff { get; }
        public LinesChangedReason ChangedReason { get; }
        public int TailStartsAt { get; }


        public LineIndicies(int[] lines, Encoding encoding, int lineFeedSize, LineIndicies previous = null)
        {
            Encoding = encoding;
            LineFeedSize = lineFeedSize;
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
                var latest = new int[previous.Lines.Length + lines.Length];
                previous.Lines.CopyTo(latest, 0);
                lines.CopyTo(latest, previous.Lines.Length);

                Lines = latest;
                Diff = lines.Length;
                ChangedReason = LinesChangedReason.Tailed;
                TailStartsAt = previous.Count - 1;
            }
        }
    }
}