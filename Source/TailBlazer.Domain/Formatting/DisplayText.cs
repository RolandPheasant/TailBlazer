namespace TailBlazer.Domain.Formatting
{

    public class DisplayText
    {
        public string Text { get; }
        public bool Highlight { get; }
        public Hue Hue { get; }
        public Hue BackgroundHue { get; } 
        public int Length => Text.Length;

        public DisplayText(MatchedString matchedString)
        {
            Text = matchedString.Part;
            Highlight = matchedString.IsMatch;
            Hue = matchedString.Hue;
            BackgroundHue = matchedString.BackgroundHue;
        }

        public DisplayText(DisplayText displayText, string text)
        {
            Text = text;
            Highlight = displayText.Highlight;
            Hue = displayText.Hue;
            BackgroundHue = displayText.BackgroundHue;
        }

    }
}
