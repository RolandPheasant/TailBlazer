using System.Reactive.Linq;
using System.IO;
using DynamicData;

namespace TailBlazer.Domain.FileHandling
{
    public interface IRecentFiles
    {
        IObservableList<RecentFile> Items { get; }

        void Register(FileInfo file);
    }
}
