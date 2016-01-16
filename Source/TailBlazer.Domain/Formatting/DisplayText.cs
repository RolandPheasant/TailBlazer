
namespace TailBlazer.Domain.Formatting
{


    public class DisplayText
    {
        public string Text { get; }
       // public Optional<Brush> Brush { get; }

        public bool Highlight { get; }

        public DisplayText(MatchedString matchedString)
        {
            Text = matchedString.Part;
            Highlight = matchedString.IsMatch;
          //  Brush = null;
        }
    }
}
