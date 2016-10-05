using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling
{
    public sealed class EmptyLineProvider: ILineProvider
    {
        public static readonly ILineProvider Instance = new EmptyLineProvider();

        public int Count { get; } = 0;
        public TailInfo TailInfo { get; } = TailInfo.Empty;

        public IEnumerable<Line> ReadLines(ScrollRequest scroll)
        {
            yield break;
        }
    }
}