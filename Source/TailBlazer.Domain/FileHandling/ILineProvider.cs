using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling
{


    public interface IHasLimitationOfLines
    {
        bool HasReachedLimit { get; }

        int Maximum { get; }
    }

    public interface ILineProvider
    {
        bool IsEmpty { get; }

        int Count { get; }

        IEnumerable<Line> ReadLines(ScrollRequest scroll);
    }

    public class EmptyLineProvider: ILineProvider
    {
        public bool IsEmpty { get; } = true;
        public int Count { get; } = 0;


        public IEnumerable<Line> ReadLines(ScrollRequest scroll)
        {
            yield break;
        }
    }
}