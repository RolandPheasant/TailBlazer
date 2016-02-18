using System;
using System.Windows.Media;
using DynamicData.Binding;
using DynamicData.Kernel;
using MaterialDesignThemes.Wpf;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Views.Tail
{
    public class LineMatchProxy : AbstractNotifyPropertyChanged
    {
        private readonly LineMatch _match;

        public LineMatchProxy(LineMatch match)
        {
            _match = match;
            IconKind = _match.Icon.ParseEnum<PackIconKind>().ValueOr(() => PackIconKind.ArrowRightBold);
        }

        public PackIconKind IconKind { get; }
        public string Description => $"Matches '{_match.Text}'";
        public string Text => _match.Text;
        public Brush Foreground => _match.Hue.ForegroundBrush;
        public Brush Background => _match.Hue.BackgroundBrush;

    }
}