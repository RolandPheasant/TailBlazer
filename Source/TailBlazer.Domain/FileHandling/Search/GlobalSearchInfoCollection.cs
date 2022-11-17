using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling.Search;

public sealed class GlobalSearchInfoCollection //: ISearchInfoCollection
{
    private readonly ISearchMetadataCollection _searchMetadataCollection;

    public GlobalSearchInfoCollection(ISearchMetadataCollection searchMetadataCollection, ILogger logger)
    {
        _searchMetadataCollection = searchMetadataCollection;

           
    }
}