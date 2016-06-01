using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Views.Searching
{
    public interface IGlobalSearchOptions
    {
        ISearchMetadataCollection MetadataCollection { get; }
    }
}