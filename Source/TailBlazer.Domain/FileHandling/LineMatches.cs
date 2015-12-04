namespace TailBlazer.Domain.FileHandling
{
    public class LineMatches
    {
        public readonly static LineMatches None = new LineMatches(); 
        public int[] Lines { get; }
        public int Count => Lines.Length;
        public int Diff { get; }
        public LineMatchChangedReason ChangedReason { get; }
        
        private LineMatches()
        {
            ChangedReason= LineMatchChangedReason.None;
        }

        public LineMatches(int[] lines, LineMatches previous=null)
        {
            if (previous == null)
            {
                Lines = lines;
                Diff = lines.Length;
                ChangedReason = LineMatchChangedReason.Loaded;
            }
            else
            {

                //combine the 2 arrays
                var latest = new int[previous.Lines.Length + lines.Length];
                previous.Lines.CopyTo(latest, 0);
                lines.CopyTo(latest, previous.Lines.Length);
                Lines = latest;

                Diff = lines.Length;
                ChangedReason = LineMatchChangedReason.Tailed;
            }
        }
    }
}