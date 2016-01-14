using System;
using DynamicData;

namespace TailBlazer.Domain.FileHandling.Search
{

    public interface ISearchMetadataCollection : IDisposable
    {
        IObservableCache<SearchMetadata, string> Metadata { get; }

        void Add(SearchMetadata metadata);
        void Remove(string searchText);
    }
}