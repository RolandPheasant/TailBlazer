using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling
{
    public interface ILineReader
    {
        int Count { get; }
        TailInfo TailInfo { get; }
        IEnumerable<Line> ReadLines(ScrollRequest scroll);
    }
}