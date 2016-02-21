using System;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Domain.FileHandling.Search
{
    public class SearchMetadataFactory : ISearchMetadataFactory
    {
        private readonly IKnownIcons _knownIcons;
        private readonly IAccentColourProvider _accentColourProvider;

        public SearchMetadataFactory(IKnownIcons knownIcons, IAccentColourProvider accentColourProvider)
        {
            _knownIcons = knownIcons;
            _accentColourProvider = accentColourProvider;
        }

        public SearchMetadata Create([NotNull] string searchText, bool useRegex, int index, bool filter)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));

            var icon = _knownIcons.GetIconFor(searchText, useRegex);


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