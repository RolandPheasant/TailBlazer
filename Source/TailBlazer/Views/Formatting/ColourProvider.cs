using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using DynamicData.Kernel;
using MaterialDesignColors;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Settings;
using Hue = TailBlazer.Domain.Formatting.Hue;

namespace TailBlazer.Views.Formatting
{

    public class ColourProvider : IColourProvider, IDisposable
    {
        private readonly IDisposable _cleanUp;

        private IDictionary<HueKey ,Hue> HueCache { get; }

        public IEnumerable<Hue> Hues { get; }

        public IObservable<Hue> AccentChanged { get; }

        public Hue DefaultAccent { get; private set; }

        public ColourProvider(ISetting<GeneralOptions> setting)
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

            AccentChanged = setting.Value
                .Select(options => options.Theme.GetAccentColor())
                .Select(colour => Hues.Last(s => s.Swatch.Equals(colour, StringComparison.OrdinalIgnoreCase)))
                .Replay(1).RefCount();

            _cleanUp = AccentChanged.Subscribe(accent => DefaultAccent = accent);
            
        }


        public Optional<Hue> Lookup(HueKey key)
        {
            return HueCache.Lookup(key);
        }
        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}