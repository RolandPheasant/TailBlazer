using System;
using System.Collections.Generic;
using DynamicData;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling.Search
{

    public interface ISearchMetadataCollection : IDisposable
    {
        IObservableCache<SearchMetadata, string> Metadata { get; }

        void Add(IEnumerable<SearchMetadata> metadata);

        void AddorUpdate(SearchMetadata metadata);
        void Remove(string searchText);

        int NextIndex();
    }
}