using DynamicData;

namespace TailBlazer.Settings
{
    public interface IRecentSearchCollection
    {
        IObservableList<RecentSearch> Items { get; }

        void Add(RecentSearch file);

        void Remove(RecentSearch file);
    }
}
