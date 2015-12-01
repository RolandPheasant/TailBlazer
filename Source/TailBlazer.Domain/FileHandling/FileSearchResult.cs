using System;

namespace TailBlazer.Domain.FileHandling
{
    public class FileSearchResult
    {
        public long Start { get; }
        public long End { get; }
        public long[] Indicies { get; }

        public FileSearchResult(long start, long end, long[] indicies)
        {
            if (indicies == null) throw new ArgumentNullException(nameof(indicies));
            Start = start;
            End = end;
            Indicies = indicies;
        }
    }
}