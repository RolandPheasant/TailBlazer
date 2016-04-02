using System;
using System.Collections.Generic;
using DynamicData;
using DynamicData.Binding;

namespace TailBlazer.Domain.FileHandling.Search
{
    public interface ISearchInfoCollection : IDisposable
    {
        IObservableCache<SearchInfo, string> Searches { get; }

        IObservable<ILineProvider> All { get; }

        void Add(string searchText, bool useRegex);
        void Remove(string searchText);
    }
}