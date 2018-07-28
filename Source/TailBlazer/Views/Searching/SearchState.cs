using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Views.Searching
{
    public sealed  class SearchState
    {
        public string Text { get; }
        public int Position { get;  }
        public bool UseRegEx { get;  }
        public HighlightingMode Highlight { get; }
        public bool Filter { get; }
        public bool Alert { get; }
        public bool IgnoreCase { get; }

        public string Swatch { get; }

        public string Hue { get; }
        public bool IsExclusion { get;  }

        public string Icon { get; }

        public SearchState(string text,int position, bool useRegEx, HighlightingMode highlight, bool filter, bool alert, 
            bool ignoreCase, string swatch, string icon, string hue, bool isExclusion)
        {
            Text = text;
            Position = position;
            UseRegEx = useRegEx;
            Highlight = highlight;
            Filter = filter;
            Alert = alert;
            IgnoreCase = ignoreCase;
            Swatch = swatch;
            Icon = icon;
            Hue = hue;
            IsExclusion = isExclusion;
        }
    }
}