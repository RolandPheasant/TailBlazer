using DynamicData;
using TailBlazer.Views.Recent;

namespace TailBlazer.Views.Searching
{
    public interface IRecentSearchCollection
    {
        IObservableList<RecentSearch> Items { get; }

        void Add(RecentSearch file);

        void Remove(RecentSearch file);
    }
}
