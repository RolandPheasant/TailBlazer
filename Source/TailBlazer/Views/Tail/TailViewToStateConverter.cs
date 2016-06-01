using System.Linq;
using System.Xml.Linq;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Settings;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Tail
{
    public class TailViewToStateConverter : IConverter<TailViewState>
    {
        private static readonly SearchMetadataToStateConverter SearchMetadataToStateConverter = new SearchMetadataToStateConverter();

        private static class Structure
        {
            public const string Root = "TailView";

            public const string FileName = "FileName";
            public const string SelectedFilter = "SelectedSearch";
        }

        //convert from the view model values

        public State Convert(string fileName, string selectedSearch, SearchMetadata[] items)
        {
            var searchItems = items
                .OrderBy(s => s.Position)
                .Select(search => new SearchState
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
                        search.HighlightHue.Name
                    )).ToArray();

            var tailViewState = new TailViewState(fileName, selectedSearch, searchItems);
            return Convert(tailViewState);
        }

        public TailViewState Convert(State state)
        {
            if (state == null || state == State.Empty)
                return GetDefaultValue();

            var doc = XDocument.Parse(state.Value);

            var root = doc.ElementOrThrow(Structure.Root);
            var filename = root.ElementOrThrow(Structure.FileName);
            var selectedFilter = root.ElementOrThrow(Structure.SelectedFilter);

            var searchStates = SearchMetadataToStateConverter.Convert(root);
            return new TailViewState(filename, selectedFilter, searchStates);
        }

        public State Convert(TailViewState state)
        {
            if (state == null || state == TailViewState.Empty)
                return State.Empty;

            var root = new XElement(new XElement(Structure.Root));
            root.Add(new XElement(Structure.FileName, state.FileName));
            root.Add(new XElement(Structure.SelectedFilter, state.SelectedSearch));

            var list = SearchMetadataToStateConverter.ConvertToElement(state.SearchItems.ToArray());
            root.Add(list);

            var doc = new XDocument(root);
            return new State(1, doc.ToString());
        }

        public TailViewState GetDefaultValue()
        {
            return TailViewState.Empty;
        }
    }
}