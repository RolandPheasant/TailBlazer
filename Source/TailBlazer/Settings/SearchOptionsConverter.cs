using System.Xml.Linq;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Settings
{
    public class SearchOptionsConverter : IConverter<SearchOptions>
    {
        private static class Structure
        {
            public const string Root = "Root";
            public const string SearchText = "SearchText";
            public const string Foreground = "Foreground";
            public const string Background = "Background";
            public const string Alert = "Alert";
        }

        public SearchOptions Convert(State state)
        {
            var defaults = this.GetDefaultValue();

            //var doc = XDocument.Parse(state.Value);
            //var root = doc.ElementOrThrow(Structure.Root);
            //var theme = root.ElementOrThrow(Structure.Foreground).ParseEnum<Theme>().ValueOr(() => defaults.Theme);
            //var highlight = root.ElementOrThrow(Structure.Background).ParseBool().ValueOr(() => defaults.HighlightTail);
            //var duration = root.ElementOrThrow(Structure.Duration).ParseDouble().ValueOr(() => defaults.HighlightDuration);
            //var scale = root.ElementOrThrow(Structure.Scale).ParseInt().ValueOr(() => defaults.Scale);
            //return new GeneralOptions(theme, highlight, duration, scale);

            return SearchOptions.None;
        }

        public State Convert(SearchOptions options)
        {
            var root = new XElement(new XElement(Structure.Root));
            root.Add(new XElement(Structure.SearchText, options.SearchText));
            root.Add(new XElement(Structure.Foreground, options.Foreground));
            root.Add(new XElement(Structure.Background, options.Background));
            root.Add(new XElement(Structure.Alert, options.Alert));
            var doc = new XDocument(root);
            var value = doc.ToString();
            return new State(1, value);
        }

        public SearchOptions GetDefaultValue()
        {
            return SearchOptions.None;
        }
    }
}