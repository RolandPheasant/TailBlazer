namespace TailBlazer.Domain.Formatting
{
    public class  GeneralOptions
    {
        public Theme Theme { get;  }
        public bool HighlightTail { get; }
        public double HighlightDuration { get; }
        public int Scale { get; }
        public int Rating { get; }

        public GeneralOptions(Theme theme, bool highlightTail, double highlightTailDuration, int scale, int rating)
        {
            Theme = theme;
            HighlightTail = highlightTail;
            HighlightDuration = highlightTailDuration;
            Scale = scale;
            Rating = rating;
        }

    }
}
