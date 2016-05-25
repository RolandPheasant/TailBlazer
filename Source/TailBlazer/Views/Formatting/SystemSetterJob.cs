using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using MaterialDesignThemes.Wpf;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Views.Formatting
{
    public sealed class SystemSetterJob: IDisposable
    {
        private readonly IDisposable _cleanUp;

        public SystemSetterJob(ISetting<GeneralOptions> setting, ISchedulerProvider schedulerProvider)
        {
             var themeSetter =  setting.Value.Select(options => options.Theme)
                .DistinctUntilChanged()
                .ObserveOn(schedulerProvider.MainThread)
                .Subscribe(theme =>
                {
                    var dark = theme == Theme.Dark;
                    var paletteHelper = new PaletteHelper();

                    paletteHelper.SetLightDark(dark);
                    paletteHelper.ReplaceAccentColor(theme.GetAccentColor());

                });

            var frameRateSetter = setting.Value.Select(options => options.FrameRate)
               .DistinctUntilChanged()
               .Take(1)
               .ObserveOn(schedulerProvider.MainThread)
               .Subscribe(frameRate =>
               {
                   Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata {DefaultValue = frameRate });

               });

            _cleanUp = new CompositeDisposable(themeSetter, frameRateSetter);
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}