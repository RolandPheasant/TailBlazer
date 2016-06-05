namespace TailBlazer.Domain.Formatting
{

    public class DisplayText
    {
        public string Text { get; }
        public bool Highlight { get; }
        public Hue Hue { get; }
        public int Length => Text.Length;

        public DisplayText(MatchedString matchedString)
        {
            Text = matchedString.Part;
            Highlight = matchedString.IsMatch;
            Hue = matchedString.Hue;
        }

        public DisplayText(DisplayText displayText, string text)
        {
            Text = text;
            Highlight = displayText.Highlight;
            Hue = displayText.Hue;
        }

    }
}
