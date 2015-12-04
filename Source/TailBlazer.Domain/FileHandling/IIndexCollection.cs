using System.Collections.Generic;
using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public interface IIndexCollection: ILineProvider
    {
        Encoding Encoding { get; }
        int Count { get; }
        int Diff { get; }
        LinesChangedReason ChangedReason { get; }
        long TailStartsAt { get; }

    }

    public interface ILineProvider
    {
        IEnumerable<LineIndex> GetIndicies(ScrollRequest scroll);
        IEnumerable<LineIndex> GetIndicies(ScrollRequest scroll, LineMatches matches);
    }
}