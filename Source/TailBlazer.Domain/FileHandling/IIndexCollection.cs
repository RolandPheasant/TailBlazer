using System.Collections.Generic;
using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public interface IIndexCollection: ILineProvider
    {
        Encoding Encoding { get; }
    }

    public interface ILineProvider
    {
        bool IsEmpty { get; }

        LinesChangedReason ChangedReason { get; }

        int Count { get; }

        IEnumerable<LineInfo> GetIndicies(ScrollRequest scroll);

    }
}