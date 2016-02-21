using System;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Domain.FileHandling.Search
{
    public class SearchMetadataFactory : ISearchMetadataFactory
    {
        private readonly IDefaultIconSelector _defaultIconSelector;
        private readonly IColourProvider _colourProvider;
        private readonly IDefaultColourSelector _defaultColourSelector;

        public SearchMetadataFactory(IDefaultIconSelector defaultIconSelector, 
            IColourProvider colourProvider,
            IDefaultColourSelector defaultColourSelector)
        {
            _defaultIconSelector = defaultIconSelector;
            _colourProvider = colourProvider;
            _defaultColourSelector = defaultColourSelector;
        }

        public SearchMetadata Create([NotNull] string searchText, bool useRegex, int index, bool filter)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));

            var icon = _defaultIconSelector.GetIconFor(searchText, useRegex);
            var hue = _defaultColourSelector.Select(searchText);

            return new SearchMetadata(index, searchText,
                filter, 
                true, 
                useRegex, 
                true,
                hue , 
                icon);
        }

    }
}