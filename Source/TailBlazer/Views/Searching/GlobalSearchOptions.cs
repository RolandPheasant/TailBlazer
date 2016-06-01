using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Views.Searching
{
    public class GlobalSearchOptions : IGlobalSearchOptions
    {
        public ISearchMetadataCollection MetadataCollection { get;  }

        public GlobalSearchOptions(ISearchMetadataCollection metadataCollection)
        {
            MetadataCollection = metadataCollection;
        }
    }
}