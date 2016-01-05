using System;
using DynamicData;

namespace TailBlazer.Domain.FileHandling.Search
{

    //TODO: This need disposing somewhere, but what is responsoble for that
    public interface ISearchMetadataCollection : IDisposable
    {
        IObservableCache<SearchMetadata, CaseInsensitiveString> Metadata { get; }

        void Add(SearchMetadata metadata);
        void Remove(string searchText);
    }
}