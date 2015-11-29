namespace TailBlazer.Domain.FileHandling
{
    public class SparseIndex
    {
        public int Start { get; }
        public int End { get; }
        public int[] Indicies { get; }
        public int Compression { get; }
        public int LineCount { get; }
        public int IndexCount => Indicies.Length;
        public int Size => Indicies[IndexCount - 1] - Start;

        public SparseIndex(int start, int end, int[] indicies, int compression, int lineCount)
        {
            Start = start;
            End = end;
            Indicies = indicies;
            Compression = compression;
            LineCount = lineCount;
        }

        public SparseIndex(int start, int end,  int compression, int lineCount)
        {
            Start = start;
            End = end;
            Indicies = new int [0];
            Compression = compression;
            LineCount = lineCount;
        }
    }
}