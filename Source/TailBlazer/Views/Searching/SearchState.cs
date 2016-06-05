namespace TailBlazer.Views.Searching
{


    public sealed  class SearchState
    {
        public string Text { get; }
        public int Position { get;  }
        public bool UseRegEx { get;  }
        public bool Highlight { get; }
        public bool Filter { get; }
        public bool Alert { get; }
        public bool IgnoreCase { get; }

        public string Swatch { get; }

        public string Hue { get; }

        public string Icon { get; }

        public SearchState(string text,int position, bool useRegEx, bool highlight, bool filter, bool alert, bool ignoreCase, string swatch, string icon, string hue)
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
        }
    }
}