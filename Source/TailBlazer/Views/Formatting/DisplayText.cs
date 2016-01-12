using System.Collections.Generic;
using System.Windows.Media;
using DynamicData.Kernel;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Views.Formatting
{

    

    public class DisplayTextCollection
    {
        public DisplayTextCollection(IEnumerable<DisplayText> displayItems )
        {
        }
    }

    public class DisplayText
    {
        public string Text { get; }
        public Optional<Brush> Brush { get; }

        public bool Highlight { get; }

        public DisplayText(MatchedString matchedString)
        {
            Text = matchedString.Part;
            Highlight = matchedString.IsMatch;
            Brush = null;
        }
    }
}
