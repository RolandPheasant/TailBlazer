using DynamicData.Kernel;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Views.Searching
{
    public class SearchStateToMetadataMapper : ISearchStateToMetadataMapper
    {
        private readonly IColourProvider _colourProvider;

        public SearchStateToMetadataMapper(IColourProvider colourProvider)
        {
            _colourProvider = colourProvider;
        }

        public SearchMetadata Map(SearchState state, bool isGlobal = false)
        {
            var hue = _colourProvider
                .Lookup(new HueKey(state.Swatch, state.Hue))
                .ValueOr(() => _colourProvider.DefaultAccent);

            return new SearchMetadata(state.Position, state.Text, state.Filter, state.Highlight, state.UseRegEx, state.IgnoreCase,
                hue,
                state.Icon,
                isGlobal,
                state.IsExclusion);
        }

        public SearchState Map(SearchMetadata search)
        {
            return new SearchState
            (
                search.SearchText,
                search.Position,
                search.UseRegex,
                search.Highlight,
                search.Filter,
                false,
                search.IgnoreCase,
                search.HighlightHue.Swatch,
                search.IconKind,
                search.HighlightHue.Name,
                search.IsExclusion);
        }
    }
}