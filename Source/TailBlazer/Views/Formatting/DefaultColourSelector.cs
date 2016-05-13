using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData;
using DynamicData.Kernel;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Views.Formatting
{
    public sealed class DefaultColourSelector : IDefaultColourSelector
    {
        private readonly IColourProvider _colourProvider;
        private readonly DefaultHue[] _defaults;

        public DefaultColourSelector(IColourProvider colourProvider)
        {
            _colourProvider = colourProvider;
            _defaults = Load().ToArray();
        }
        
        public Hue Select(string text)
        {
            var match = _defaults
                .FirstOrDefault(hue => hue.MatchTextOnCase
                    ? hue.Text.Equals(text)
                    : hue.Text.Equals(text, StringComparison.OrdinalIgnoreCase));

            return match != null ? match.Hue : _colourProvider.DefaultAccent;
        }

        public Hue Lookup(HueKey key)
        {
            return _colourProvider.Lookup(key).ValueOr(() => _colourProvider.DefaultAccent);
        }

        private IEnumerable<DefaultHue> Load()
        {
            yield return new DefaultHue("DEBUG", Lookup("blue", "Accent400"));
            yield return new DefaultHue("INFO", Lookup("deeppurple", "Accent200"));
            yield return new DefaultHue("WARN", Lookup("orange", "Accent700"));
            yield return new DefaultHue("WARNING", Lookup("orange", "Accent700"));
            yield return new DefaultHue("ERROR", Lookup("red", "Accent400"));
            yield return new DefaultHue("FATAL", Lookup("red", "Accent700"));

        }

        private Hue Lookup(string swatch, string name)
        {
            return _colourProvider.Lookup(new HueKey(swatch, name))
                .ValueOrThrow(() => new MissingKeyException(swatch + "."+ name + " is invalid"));
        }

        private class DefaultHue
        {
            public string Text { get; }
            public Hue Hue { get; }
            public bool MatchTextOnCase { get; }

            public DefaultHue(string text, Hue hue, bool matchTextOnCase = false)
            {
                Text = text;
                Hue = hue;
                MatchTextOnCase = matchTextOnCase;
            }
        }
    }
}