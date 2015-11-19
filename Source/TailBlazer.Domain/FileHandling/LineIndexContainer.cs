namespace TailBlazer.Domain.FileHandling
{
    public class LineIndexContainer
    {
        public bool IsInitial { get; }
        public int[] Lines { get; }
        public int LineCount => Lines.Length;
        public int Diff { get; }
        public int EndOfTail { get; }

        public LineIndexContainer(int[] lines, LineIndexContainer previous = null)
        {
            if (previous == null)
            {
                Lines = lines;
                IsInitial = true;
                Diff = lines.Length;
            }
            else
            {

                //combine the 2 arrays
                var latest = new int[previous.Lines.Length + lines.Length];
                previous.Lines.CopyTo(latest, 0);
                lines.CopyTo(latest, previous.Lines.Length);
                Lines = latest;

                IsInitial = false;
                Diff = lines.Length;
            }
        }
    }
}