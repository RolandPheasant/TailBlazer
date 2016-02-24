using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Views.Tail;

namespace TailBlazer.Views.Searching
{
    public interface ISearchStateToMetadataMapper
    {
        SearchMetadata Map(SearchState state);
    }
}