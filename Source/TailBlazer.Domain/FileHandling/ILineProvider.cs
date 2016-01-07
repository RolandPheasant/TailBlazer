using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling
{
    public interface ILineProvider
    {
        //bool IsEmpty { get; }

        int Count { get; }

        IEnumerable<Line> ReadLines(ScrollRequest scroll);
    }
}