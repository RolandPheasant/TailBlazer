using System.Collections.Generic;
using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public interface IIndexCollection
    {
        Encoding Encoding { get; }
        int Count { get; }
        int Diff { get; }
        LinesChangedReason ChangedReason { get; }
        int TailStartsAt { get; }
        
        IEnumerable<LineIndex> GetIndicies(ScrollRequest scroll);
        IEnumerable<LineIndex> GetIndicies(ScrollRequest scroll, LineMatches matches);
    }
}