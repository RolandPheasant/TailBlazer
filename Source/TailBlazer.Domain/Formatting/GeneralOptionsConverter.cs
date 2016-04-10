using System;
using System.Xml.Linq;
using DynamicData.Kernel;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Domain.Formatting
{
    public class GeneralOptionsConverter: IConverter<GeneralOptions>
    {
        private static class Structure
        {
            public const string Root = "Root";
            public const string Theme = "Theme";
            public const string HighlightTail = "HighlightTail";
            public const string Duration = "Duration";
            public const string Scale = "Scale";
        }

        public GeneralOptions Convert(State state)
        {
            var defaults = this.GetDefaultValue();
            if (state== State.Empty) return defaults;

            var doc = XDocument.Parse(state.Value);
            var root = doc.ElementOrThrow(Structure.Root);
            var theme = root.ElementOrThrow(Structure.Theme).ParseEnum<Theme>().ValueOr(()=> defaults.Theme);
            var highlight = root.ElementOrThrow(Structure.HighlightTail).ParseBool().ValueOr(() => defaults.HighlightTail);
            var duration = root.ElementOrThrow(Structure.Duration).ParseDouble().ValueOr(()=>defaults.HighlightDuration);
            var scale = root.ElementOrThrow(Structure.Scale).ParseInt().ValueOr(()=>defaults.Scale);
            return new GeneralOptions(theme,highlight, duration,scale);
        }

        public State Convert(GeneralOptions options)
        {
            var root = new XElement(new XElement(Structure.Root));
            root.Add(new XElement(Structure.Theme, options.Theme));
            root.Add(new XElement(Structure.HighlightTail, options.HighlightTail));
            root.Add(new XElement(Structure.Duration, options.HighlightDuration));
            root.Add(new XElement(Structure.Scale, options.Scale));
            var doc = new XDocument(root);
            var value= doc.ToString();
            return new State(1, value);
        }

        public GeneralOptions GetDefaultValue()
        {
            return new GeneralOptions(Theme.Light, true, 5,100);
        }
    }
}