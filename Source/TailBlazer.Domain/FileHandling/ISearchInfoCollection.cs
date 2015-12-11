using System;
using DynamicData;

namespace TailBlazer.Domain.FileHandling
{
    public interface ISearchInfoCollection : IDisposable
    {
        IObservableCache<SearchInfo, CaseInsensitiveString> Searches { get; }

        void Add(string searchText, IObservable<ILineProvider> latest, bool isDefault=false);
        void Remove(string searchText);
    }
}