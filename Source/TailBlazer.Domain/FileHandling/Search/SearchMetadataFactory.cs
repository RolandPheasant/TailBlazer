using System;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Domain.FileHandling.Search
{
    public class SearchMetadataFactory : ISearchMetadataFactory
    {
        private readonly IKnownIconNames _knownIconNames;
        private readonly IAccentColourProvider _accentColourProvider;

        public SearchMetadataFactory(IKnownIconNames knownIconNames, IAccentColourProvider accentColourProvider)
        {
            _knownIconNames = knownIconNames;
            _accentColourProvider = accentColourProvider;
        }

        public SearchMetadata Create([NotNull] string searchText, bool useRegex, int index, bool filter)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));

            var icon = _knownIconNames.SelectIconFor(searchText, useRegex);


            return new SearchMetadata(index, searchText,
                filter, 
                true, 
                useRegex, 
                true, 
                _accentColourProvider.DefaultHighlight, 
                icon);
        }

    }
}