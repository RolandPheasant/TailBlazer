using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling
{
    public interface ILineReader
    {
        int Count { get; }

        IEnumerable<Line> ReadLines(ScrollRequest scroll);
    }
}