using System.Collections;
using System.Linq;
using System.Xml.Linq;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Views.Tail
{
    public class TailViewPersister: IPersistentStateProvider
    {
        private readonly TailViewModel _tailView;

        public TailViewPersister(TailViewModel tailView)
        {
            _tailView = tailView;
        }

        public State CaptureState()
        {

            var searchItems = _tailView.SearchOptions
                                .Data
                                .Select(search => new SearchState
                                    (
                                        search.Text,
                                        search.UseRegex,
                                        search.Highlight,
                                        search.Filter,
                                        false,
                                        search.IgnoreCase
                                    ));

            var tailViewState = new TailViewState(_tailView.Name,_tailView.SearchCollection.Selected.Text, searchItems);
            var converter = new TailViewToStateConverter();
            return converter.Convert(tailViewState);
        }

        public void Restore(State state)
        {
            
        }
    }

    public sealed  class SearchState
    {
        public string Text { get; }
        public bool UseRegEx { get;  }
        public bool Highlight { get; }
        public bool Filter { get; }
        public bool Alert { get; }
        public bool IgnoreCase { get; }

        public SearchState(string text,bool useRegEx, bool highlight, bool filter, bool alert, bool ignoreCase)
        {
            Text = text;
            UseRegEx = useRegEx;
            Highlight = highlight;
            Filter = filter;
            Alert = alert;
            IgnoreCase = ignoreCase;
        }
    }

    public class TailViewToStateConverter : IConverter<TailViewState>
    {
        private static class Structure
        {
            public const string Root = "TailView";

            public const string FileName = "FileName";
            public const string SelectedFilter = "SelectedSearch";

            public const string SearchList = "SearchList";
            public const string SearchItem = "SearchItem";

            public const string Text = "Text";
            public const string Filter = "Filter";
            public const string UseRegEx = "UseRegEx";
            public const string Highlight = "Highlight";
            public const string Alert = "Alert";
            public const string IgnoreCase = "IgnoreCase";
        }

        public TailViewState Convert(State state)
        {
            if (state == null || state == State.Empty)
                return GetDefaultValue();

            var doc = XDocument.Parse(state.Value);



            var root = doc.ElementOrThrow(Structure.Root);

            //var files = root.Elements(Structure.SearchList)
            //                .Select(element =>
            //                {
            //                    var name = element.Attribute(Structure.Text).Value;
            //                    var dateTime = element.Attribute(Structure.Highlight).Value;
            //                    return new RecentSearch(bool.Parse(dateTime), name);
            //                }).ToArray();
            return null;
        }

        public State Convert(TailViewState state)
        {
            if (state == null || state == TailViewState.Empty)
                return State.Empty;

            var root = new XElement(new XElement(Structure.Root));
            root.Add(new XElement(Structure.FileName, state.FileName));
            root.Add(new XElement(Structure.SelectedFilter, state.SelectedSearch));

            var list = new XElement(Structure.SearchList);

            var searchItemsArray = state.SearchItems.Select(f => new XElement(Structure.SearchItem,
                new XAttribute(Structure.Text, f.Text),
                new XAttribute(Structure.Filter, f.Filter),
                new XAttribute(Structure.UseRegEx, f.UseRegEx),
                new XAttribute(Structure.Highlight, f.Highlight),
                new XAttribute(Structure.Alert, f.Alert),
                new XAttribute(Structure.IgnoreCase, f.IgnoreCase)));

            searchItemsArray.ForEach(list.Add);

            root.Add(list);

            XDocument doc = new XDocument(root);
            return new State(1, doc.ToString());
        }

        public TailViewState GetDefaultValue()
        {
            return TailViewState.Empty;
        }
    }
}
