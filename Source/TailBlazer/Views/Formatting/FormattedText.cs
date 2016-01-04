using System;
using System.Windows.Media;
using DynamicData.Kernel;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Views.Formatting
{
    public class FormattedText
    {
        public string Text { get; }
        public Optional<Brush> Brush { get; }


        public bool Highlight { get; }

        public FormattedText(MatchedString matchedString)
        {
            Text = matchedString.Part;
            Highlight = matchedString.IsMatch;
            Brush = null;


        }
    }
}