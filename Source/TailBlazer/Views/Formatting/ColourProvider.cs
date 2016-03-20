using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DynamicData.Kernel;
using MaterialDesignColors;
using TailBlazer.Domain.Formatting;
using Hue = TailBlazer.Domain.Formatting.Hue;

namespace TailBlazer.Views.Formatting
{
    public class ColourProvider : IColourProvider
    {
        private IDictionary<HueKey ,Hue> HueCache { get; }

        public IEnumerable<Hue> Hues { get; }

        public Hue DefaultAccent => Hue.NotSpecified;

        public ColourProvider()
        {
            var swatches = new SwatchesProvider()
                .Swatches.Where(s => s.IsAccented).ToArray();

            var orders = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            ThemeConstants.Themes.Select((str,idx)=>new {str,idx})
                .ForEach(x=> orders[x.str] = x.idx);

            Hues = swatches
                        .OrderBy(x => orders.Lookup(x.Name).ValueOr(() => 100))
                        .SelectMany(swatch =>
                        {
                            return swatch.AccentHues.Select(hue => new Hue(swatch.Name, hue.Name, hue.Foreground, hue.Color));
                        })
                        .ToArray();

            HueCache = Hues.ToDictionary(h => h.Key);
        }


        public Optional<Hue> Lookup(HueKey key)
        {
            return HueCache.Lookup(key);
        }

    }
}