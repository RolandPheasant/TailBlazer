using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling
{
    public interface ILineReader
    {
        int Diff { get; } //get this from segment instead???

        int Count { get; }

        IEnumerable<Line> ReadLines(ScrollRequest scroll);
    }
}