using System;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Views.Searching;

public interface ISearchProxyCollectionFactory
{
    ISearchProxyCollection Create(ISearchMetadataCollection metadataCollection, Guid id, Action<SearchMetadata> changeScopeAction);
}