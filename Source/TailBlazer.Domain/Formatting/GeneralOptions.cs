namespace TailBlazer.Domain.Formatting
{
    public class  GeneralOptions
    {
        public Theme Theme { get;  }
        public bool HighlightTail { get; }
        public double HighlightDuration { get; }
        public double Scale { get; }
        public int Rating { get; }
        public bool OpenRecentOnStartup { get; }

        public GeneralOptions(Theme theme, bool highlightTail, double highlightTailDuration, double scale, int rating, bool openRecentOnStartup)
        {
            Theme = theme;
            HighlightTail = highlightTail;
            HighlightDuration = highlightTailDuration;
            Scale = scale;
            Rating = rating;
            OpenRecentOnStartup = openRecentOnStartup;
        }

    }
}
