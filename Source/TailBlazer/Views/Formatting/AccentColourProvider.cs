using System;
using System.Collections.Generic;
using System.Linq;
using MaterialDesignColors;
using TailBlazer.Domain.Formatting;
using Hue = TailBlazer.Domain.Formatting.Hue;

namespace TailBlazer.Views.Formatting
{
    public class AccentColourProvider : IAccentColourProvider
    {
        public IEnumerable<Hue> Hues { get; }
        public Hue DefaultHighlight { get; }

        public AccentColourProvider()
        {

            var swatches = new SwatchesProvider()
                .Swatches.Where(s => s.IsAccented).ToArray();
            
            Hues = swatches
                .Select(s=>s.AccentExemplarHue)
                .Select(h=>new Hue(h.Foreground, h.Color))
                .ToArray();

            var defaultHighlight = swatches
                .First(s => s.Name.Equals("amber",StringComparison.OrdinalIgnoreCase));

            DefaultHighlight = new Hue(defaultHighlight.ExemplarHue.Foreground, defaultHighlight.ExemplarHue.Color);
    
        }


    }
}