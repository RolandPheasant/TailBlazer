using System;
using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling
{
    public interface ILineProvider : IComparable
    {
        //bool IsEmpty { get; }

        int Count { get; }

        IEnumerable<Line> ReadLines(ScrollRequest scroll);
    }
}