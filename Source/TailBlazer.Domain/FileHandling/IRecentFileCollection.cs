using System.Reactive.Linq;
using System.IO;
using DynamicData;

namespace TailBlazer.Domain.FileHandling
{
    public interface IRecentFileCollection
    {
        IObservableList<RecentFile> Items { get; }

        void Add(RecentFile file);

        void Remove(RecentFile file);
    }
}
