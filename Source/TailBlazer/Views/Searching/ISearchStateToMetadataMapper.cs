using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Views.Searching
{
    public interface ISearchStateToMetadataMapper
    {
        SearchMetadata Map(SearchState state);
    }
}