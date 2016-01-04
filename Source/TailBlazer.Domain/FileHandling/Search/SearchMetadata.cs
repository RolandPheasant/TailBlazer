using System;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling.Search
{
    public class SearchMetadata
    {
        public string SearchText { get; }
        public bool Filter { get; }
        public bool Highlight { get; }

        public SearchMetadata([NotNull] string searchText, bool filter, bool highlight)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));

            SearchText = searchText;
            Filter = filter;
            Highlight = highlight;
        }
    }
}