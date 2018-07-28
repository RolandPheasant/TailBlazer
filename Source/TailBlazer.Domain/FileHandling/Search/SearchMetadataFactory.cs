using System;
using TailBlazer.Domain.FileHandling.TextAssociations;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Domain.FileHandling.Search
{
    public class SearchMetadataFactory : ISearchMetadataFactory
    {
        private readonly IDefaultIconSelector _defaultIconSelector;
        private readonly IDefaultColourSelector _defaultColourSelector;
        private readonly ITextAssociationCollection _textAssociationCollection;

        public SearchMetadataFactory(IDefaultIconSelector defaultIconSelector, 
            IDefaultColourSelector defaultColourSelector,
            ITextAssociationCollection textAssociationCollection)
        {
            _defaultIconSelector = defaultIconSelector;
            _defaultColourSelector = defaultColourSelector;
            _textAssociationCollection = textAssociationCollection;
        }

        public SearchMetadata Create(string searchText, bool useRegex, int index, bool filter, bool isGlobal = false)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));

            var isExclusion = false;
            if (!useRegex)
            {
                var withNegation = searchText.WithNegation();
                isExclusion = withNegation.IsNegation;
                searchText = withNegation.Text;
            }

            var association = _textAssociationCollection.Lookup(searchText);
            string icon;
            Hue hue;

            if (association.HasValue)
            {
                icon =  association.Value.Icon;
                hue = _defaultColourSelector.Lookup(new HueKey(association.Value.Swatch, association.Value.Hue));
            }
            else
            {
                icon = _defaultIconSelector.GetIconFor(searchText, useRegex);
                hue = _defaultColourSelector.Select(searchText);
            }
            
            return new SearchMetadata(index, searchText,
               isExclusion ? false: filter, 
                HighlightingMode.Text, 
                useRegex, 
                true,
                hue , 
                icon,
                isGlobal,
                isExclusion);
        }
    }
}