using System;
using DynamicData;

namespace TailBlazer.Domain.FileHandling.Search
{

    public interface ISearchMetadataCollection : IDisposable
    {
        IObservableCache<SearchMetadata, CaseInsensitiveString> Metadata { get; }

        void Add(SearchMetadata metadata);
        void Remove(string searchText);
    }
}