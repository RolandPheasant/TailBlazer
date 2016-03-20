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

        public SearchMetadata Map(SearchState state)
        {
            var hue = _colourProvider
                .Lookup(new HueKey(state.Swatch, state.Hue))
                .ValueOr(()=> _colourProvider.DefaultAccent);
                        
            return new SearchMetadata(state.Position, state.Text, state.Filter,state.Highlight, state.UseRegEx,state.IgnoreCase,
                hue,
                state.Icon);
        }
    }
}