using System;
using System.Reactive.Linq;
using MaterialDesignThemes.Wpf;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Views.Formatting
{
    public sealed class ThemeSetterJob: IDisposable
    {
        private readonly IDisposable _cleanUp;

        public ThemeSetterJob(ISetting<GeneralOptions> setting, ISchedulerProvider schedulerProvider)
        {
            _cleanUp =  setting.Value.Select(options => options.Theme)
                .ObserveOn(schedulerProvider.MainThread)
                .Subscribe(theme =>
                {
                    var dark = theme == Theme.Dark;
                    var paletteHelper = new PaletteHelper();

                    paletteHelper.SetLightDark(dark);
                    paletteHelper.ReplaceAccentColor(theme.GetAccentColor());

                });
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}