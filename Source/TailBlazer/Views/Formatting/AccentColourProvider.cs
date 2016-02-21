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
    public class AccentColourProvider : IAccentColourProvider
    {
        public IEnumerable<Hue> Hues { get; }
        public Hue DefaultHighlight { get; }

        private readonly IDictionary<string, int> _orders = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            {"yellow", 1},
            {"amber", 2},


            {"lightgreen", 5},
            {"green", 6},
            {"lime", 7},
            {"teal", 8},

            {"cyan", 9},
            {"lightblue", 10},
            {"blue", 11},
            {"indigo", 12},

                        {"orange", 23},
            {"deeporange", 24},
                 {"pink", 25},
                  {"red", 26},
            {"purple", 27},

            {"deeppurple", 28},

        };

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

        public AccentColourProvider()
        {

            var swatches = new SwatchesProvider()
                .Swatches.Where(s => s.IsAccented).ToArray();

            var orders = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            _order.Select((str,idx)=>new {str,idx})
                
                .ForEach(x=> orders[x.str] = x.idx);

            Hues = swatches
                .OrderBy(x =>
                {
                    var order = orders.Lookup(x.Name)
                        .ValueOr(() =>
                        {
                            Console.WriteLine(x);
                            return 100;
                        } );

                    return order;
                })
               .SelectMany(s => s.AccentHues)
               .Select(h => new Hue(h.Foreground, h.Color))
               .ToArray();

            //Hues = swatches
            //    .Select(s => s.AccentExemplarHue)
            //    .Select(h => new Hue(h.Foreground, h.Color))
            //    .ToArray();

            var defaultHighlight = swatches
                .First(s => s.Name.Equals("amber",StringComparison.OrdinalIgnoreCase));

            DefaultHighlight = new Hue(defaultHighlight.ExemplarHue.Foreground, defaultHighlight.ExemplarHue.Color);
    
        }



    }
}