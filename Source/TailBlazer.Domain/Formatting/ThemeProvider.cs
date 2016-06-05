using System;
using System.Reactive.Linq;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Domain.Formatting
{
    public class ThemeProvider: IThemeProvider
    {
        public IObservable<Theme> Theme { get; }

        public IObservable<Hue> Accent { get; }

        public ThemeProvider([NotNull] ISetting<GeneralOptions> setting, [NotNull] IColourProvider colourProvider)
        {
            if (setting == null) throw new ArgumentNullException(nameof(setting));
            if (colourProvider == null) throw new ArgumentNullException(nameof(colourProvider));

            Theme = setting.Value
                .Select(options => options.Theme)
                .Replay(1).RefCount();

            Accent = setting.Value
                .Select(options => colourProvider.GetAccent(options.Theme))
                .Replay(1).RefCount();
        }
    }
}