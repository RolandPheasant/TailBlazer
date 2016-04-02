using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling.Search
{
    public interface ISearchMetadataFactory
    {
        SearchMetadata Create([NotNull] string searchText, bool useRegex, int index, bool filter);
    }
}