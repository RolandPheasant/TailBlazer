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
        public IEnumerable<Hue> Hues { get; }
        public Hue DefaultHighlight { get; }

        private readonly string[] _order = new[]
        {
            "yellow",
            "amber",
            "lightgreen",
            "green",
            "lime",
            "teal",
            "cyan",
            "lightblue",
            "blue",
            "indigo",
            "orange",
            "deeporange",
            "pink",
            "red",
            "purple",
            "deeppurple",
        };

        public ColourProvider()
        {

            var swatches = new SwatchesProvider()
                .Swatches.Where(s => s.IsAccented).ToArray();

            var orders = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            _order.Select((str,idx)=>new {str,idx})
                
                .ForEach(x=> orders[x.str] = x.idx);

            Hues = swatches
                        .OrderBy(x => orders.Lookup(x.Name).ValueOr(() => 100))
                        .SelectMany(swatch =>
                        {
                            return swatch.AccentHues.Select(hue => new Hue(swatch.Name, hue.Name, hue.Foreground, hue.Color));
                        })
                        .ToArray();

            DefaultHighlight = Hues
                .Last(s => s.Swatch.Equals("amber",StringComparison.OrdinalIgnoreCase));
    
        }



    }
}