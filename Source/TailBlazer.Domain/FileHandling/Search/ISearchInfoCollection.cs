using System;
using DynamicData;

namespace TailBlazer.Domain.FileHandling.Search
{
    public interface ISearchInfoCollection : IDisposable
    {
        IObservableCache<SearchInfo, CaseInsensitiveString> Searches { get; }

        IObservable<ILineProvider> All { get; }

        void Add(string searchText);
        void Remove(string searchText);
    }
}