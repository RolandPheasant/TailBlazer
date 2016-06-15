using System;
using System.IO;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Domain.FileHandling.TextAssociations
{
    public sealed  class TextAssociation
    {
        public string Text { get; }
        public bool IgnoreCase { get;  }
        public bool UseRegEx { get;  }
        public string Swatch { get; }
        public string Hue { get; }
        public DateTime DateTime { get;  }
        public string Icon { get; }

        public TextAssociation(string text, bool ignoreCase, bool useRegEx, string swatch, string icon, string hue, DateTime dateTime)
        {
            Text = text;
            IgnoreCase = ignoreCase;
            UseRegEx = useRegEx;
            Swatch = swatch;
            Icon = icon;
            Hue = hue;
            DateTime = dateTime;
        }
    }
}