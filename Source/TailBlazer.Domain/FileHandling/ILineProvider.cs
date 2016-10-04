using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling
{
    public interface ILineProvider
    {
        int Count { get; }

        FileTailInfo TailInfo { get; }

        IEnumerable<Line> ReadLines(ScrollRequest scroll);
    }
}