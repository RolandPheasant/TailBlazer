using DynamicData;

namespace TailBlazer.Domain.FileHandling.Recent
{
    public interface IRecentFileCollection
    {
        IObservableList<RecentFile> Items { get; }

        void Add(RecentFile file);

        void Remove(RecentFile file);
    }
}
