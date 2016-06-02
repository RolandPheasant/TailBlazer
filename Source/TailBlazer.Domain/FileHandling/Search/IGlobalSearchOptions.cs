namespace TailBlazer.Domain.FileHandling.Search
{
    public interface IGlobalSearchOptions
    {
        ISearchMetadataCollection Metadata { get; }
    }
}