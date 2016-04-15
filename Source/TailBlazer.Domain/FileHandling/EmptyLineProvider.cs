using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling
{
    public class EmptyLineProvider: ILineProvider
    {
        public int Count { get; } = 0;
        public ILineProvider Previous { get; } = null;
        public ILineProvider Next { get; set; } = null;
        public int NumberOfPreviousProvider { get; } = 0;

        public IEnumerable<Line> ReadLines(ScrollRequest scroll)
        {
            yield break;
        }
    }
}