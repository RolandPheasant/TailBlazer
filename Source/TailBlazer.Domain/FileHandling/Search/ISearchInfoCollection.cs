using System;
using DynamicData;

namespace TailBlazer.Domain.FileHandling.Search
{
    public interface ISearchInfoCollection : IDisposable
    {
        IObservableCache<SearchInfo, CaseInsensitiveString> Searches { get; }

        void Add(string searchText, IObservable<ILineProvider> latest, SearchType type = SearchType.User);
        void Remove(string searchText);
    }
}