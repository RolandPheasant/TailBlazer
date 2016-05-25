

using System.Windows.Media.Animation;

namespace TailBlazer.Domain.Formatting
{
    public class  GeneralOptions
    {
        public Theme Theme { get;  }
        public bool HighlightTail { get; }
        public double HighlightDuration { get; }
        public int Scale { get; }
        public int FrameRate { get; }
        public double RefreshPeriod { get; }

        public GeneralOptions(Theme theme, bool highlightTail, double highlightTailDuration, int scale, int frameRate, double refreshPeriod)
        {
            Theme = theme;
            HighlightTail = highlightTail;
            HighlightDuration = highlightTailDuration;
            Scale = scale;
            FrameRate = frameRate;
            RefreshPeriod = refreshPeriod;
        }

    }
}
