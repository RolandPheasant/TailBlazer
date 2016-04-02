using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling
{
    public class EmptyLineProvider: ILineProvider
    {
        public int Count { get; } = 0;
        
        public IEnumerable<Line> ReadLines(ScrollRequest scroll)
        {
            yield break;
        }

        public int CompareTo(object obj)
        {
            var other = obj as EmptyLineProvider;
            return Count.CompareTo(other?.Count);
        }
    }
}