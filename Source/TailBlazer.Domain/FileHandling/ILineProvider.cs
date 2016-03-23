using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling
{
    public interface ILineProvider
    {
        int Count { get; }

        IEnumerable<Line> ReadLines(ScrollRequest scroll);
    }
}