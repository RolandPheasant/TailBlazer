using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Media;
using DynamicData.Binding;
using DynamicData.Kernel;
using MaterialDesignThemes.Wpf;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Views.Tail
{
    public sealed class LineMatchProxy : AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly LineMatch _match;
        private readonly IDisposable _cleanUp;

        public LineMatchProxy(LineMatch match, IThemeProvider themeProvider)
        {
            _match = match;
            IconKind = _match.Icon.ParseEnum<PackIconKind>().ValueOr(() => PackIconKind.ArrowRightBold);

            var defaultHue = themeProvider.Accent.Select(hue => match.Hue == Hue.NotSpecified ? hue : _match.Hue);

            Foreground = defaultHue.Select(h => h.ForegroundBrush).ForBinding();
            Background = defaultHue.Select(h => h.BackgroundBrush).ForBinding();

            _cleanUp = new CompositeDisposable(Foreground, Background);
        }
        
        public PackIconKind IconKind { get; }
        public string Description => $"Matches '{_match.Text}'";
        public string Text => _match.Text;
        
        public IProperty<Brush> Background { get; }
        public IProperty<Brush> Foreground { get; }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}