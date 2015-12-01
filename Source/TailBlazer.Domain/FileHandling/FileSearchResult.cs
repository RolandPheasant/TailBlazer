using System;

namespace TailBlazer.Domain.FileHandling
{
    public class FileSearchResult
    {
        public int Start { get; }
        public int End { get; }
        public int[] Indicies { get; }

        public FileSearchResult(int start, int end, int[] indicies)
        {
            if (indicies == null) throw new ArgumentNullException(nameof(indicies));
            Start = start;
            End = end;
            Indicies = indicies;
        }
    }
}