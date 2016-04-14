using System;
using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling
{
    struct LastValueWrapper
    {
        public long LastEndPosition { get; set; }
        public int LastPageIndex { get; set; }
        public int LastMatchesSize { get; set; }
    }

    public interface ILineProvider
    {
        //bool IsEmpty { get; }

        int Count { get; }

        ILineProvider Previous { get; }
        ILineProvider Next { get; set; }
        int NumberOfPreviousProvider { get; }

        IEnumerable<Line> ReadLines(ScrollRequest scroll);
    }
}