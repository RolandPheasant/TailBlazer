using System;

namespace TailBlazer.Domain.FileHandling;

public class FileSegmentSearchResult
{
    public long Start { get; }
    public long End { get; }
    public long[] Indicies { get; }

    public FileSegmentSearchResult(long start, long end)
    {
        Start = start;
        End = end;
        Indicies = new long[0];
    }

    public FileSegmentSearchResult(long start, long end, long[] indicies)
    {
        if (indicies == null) throw new ArgumentNullException(nameof(indicies));
        Start = start;
        End = end;
        Indicies = indicies;
    }
}