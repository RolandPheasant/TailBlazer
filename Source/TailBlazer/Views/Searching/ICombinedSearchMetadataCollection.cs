using System;
using DynamicData;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Views.Searching
{
    public interface ICombinedSearchMetadataCollection : IDisposable
    {
        IObservableCache<SearchMetadata, string> Metadata { get; }
    }
}