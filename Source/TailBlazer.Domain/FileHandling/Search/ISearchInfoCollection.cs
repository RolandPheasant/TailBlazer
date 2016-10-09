using System;
using DynamicData;

namespace TailBlazer.Domain.FileHandling.Search
{
    public interface ISearchInfoCollection : IDisposable
    {
        IObservableCache<SearchInfo, string> Searches { get; }

        IObservable<Func<string, bool>> AllFilter { get; }
        
        void Add(string searchText, bool useRegex);
        void Remove(string searchText);
    }
}