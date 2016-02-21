using System;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Domain.FileHandling.Search
{
    public class SearchMetadataFactory : ISearchMetadataFactory
    {
        private readonly IDefaultIconSelector _defaultIconSelector;
        private readonly IColourProvider _colourProvider;
        private readonly IColourSelector _colourSelector;

        public SearchMetadataFactory(IDefaultIconSelector defaultIconSelector, 
            IColourProvider colourProvider,
            IColourSelector colourSelector)
        {
            _defaultIconSelector = defaultIconSelector;
            _colourProvider = colourProvider;
            _colourSelector = colourSelector;
        }

        public SearchMetadata Create([NotNull] string searchText, bool useRegex, int index, bool filter)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));

            var icon = _defaultIconSelector.GetIconFor(searchText, useRegex);
            var hue = _colourSelector.SelectFor(searchText);

            return new SearchMetadata(index, searchText,
                filter, 
                true, 
                useRegex, 
                true,
                hue, 
                icon);
        }

    }
}