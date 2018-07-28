
using System.Collections;
using System.Linq;
using System.Xml.Linq;
using DynamicData.Kernel;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Tail
{
    public class SearchMetadataToStateConverter : IConverter<SearchState[]>
    {
        private static class Structure
        {
            public const string Root = "TailView";

            public const string SearchList = "SearchList";
            public const string SearchItem = "SearchItem";

            public const string Text = "Text";
            public const string Filter = "Filter";
            public const string UseRegEx = "UseRegEx";
            public const string Highlight = "Highlight";
            public const string Alert = "Alert";
            public const string IgnoreCase = "IgnoreCase";
            public const string Exclusion = "Exclusion";

            public const string Swatch = "Swatch";
            public const string Hue = "Hue";
            public const string Icon = "Icon";
        }

        public SearchState[] Convert(State state)
        {
            if (state == null || state == State.Empty)
                return GetDefaultValue();
                
            var doc = XDocument.Parse(state.Value);
            var root = doc.Element(Structure.Root);

            if (root!=null)
                return Convert(root);

            var searchStates = ConvertSearchList(doc.Element(Structure.SearchList)); ;
            return searchStates;
        }

        public SearchState[] Convert(XElement root)
        {
            return ConvertSearchList(root.Element(Structure.SearchList));
        }

        private SearchState[] ConvertSearchList(XElement root)
        {
            return root
             .Elements(Structure.SearchItem)
             .Select((element, index) =>
             {
                 var text = element.ElementOrThrow(Structure.Text);
                 var position = element.Attribute(Structure.Filter).Value.ParseInt().ValueOr(() => index);
                 var filter = element.Attribute(Structure.Filter).Value.ParseBool().ValueOr(() => true);
                 var useRegEx = element.Attribute(Structure.UseRegEx).Value.ParseBool().ValueOr(() => false);
                 var highlight = element.Attribute(Structure.Highlight).Value.ParseEnum<HighlightingMode>().ValueOr(() => HighlightingMode.Disabled);
                 var alert = element.Attribute(Structure.Alert).Value.ParseBool().ValueOr(() => false);
                 var ignoreCase = element.Attribute(Structure.IgnoreCase).Value.ParseBool().ValueOr(() => true);

                 bool exclusion = false;
                 var exclusionAttribute = element.Attribute(Structure.Exclusion);
                 if (exclusionAttribute != null)
                 {
                     exclusion = exclusionAttribute.Value.ParseBool().ValueOr(() => false);
                 }

                 var swatch = element.Attribute(Structure.Swatch).Value;
                 var hue = element.Attribute(Structure.Hue).Value;
                 var icon = element.Attribute(Structure.Icon).Value;

                 return new SearchState(text, position, useRegEx, highlight, filter, alert, ignoreCase, swatch, icon, hue, exclusion);
             }).ToArray();
        }

        public State Convert(SearchState[] state)
        {
            var list = ConvertToElement(state);
            XDocument doc = new XDocument(list);
            return new State(1, doc.ToString());
        }

        public XElement ConvertToElement(SearchState[] state)
        {
            var list = new XElement(Structure.SearchList);

            var searchItemsArray = state.Select(s => new XElement(Structure.SearchItem,
                new XElement(Structure.Text, s.Text),
                new XAttribute(Structure.Filter, s.Filter),
                new XAttribute(Structure.UseRegEx, s.UseRegEx),
                new XAttribute(Structure.Highlight, s.Highlight),
                new XAttribute(Structure.Alert, s.Alert),
                new XAttribute(Structure.IgnoreCase, s.IgnoreCase),
                new XAttribute(Structure.Swatch, s.Swatch),
                new XAttribute(Structure.Hue, s.Hue),
                new XAttribute(Structure.Icon, s.Icon),
                new XAttribute(Structure.Exclusion, s.IsExclusion)));

            searchItemsArray.ForEach(list.Add);
            return list;
        }

        public SearchState[] GetDefaultValue()
        {
            return Enumerable.Empty<SearchState>().ToArray();
        }
    }
}
