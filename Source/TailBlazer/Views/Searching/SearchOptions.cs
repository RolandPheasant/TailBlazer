using System.Windows.Media;

namespace TailBlazer.Views.Searching
{
    public class SearchOptions
    {
        public static readonly SearchOptions None = new SearchOptions();

        public string SearchText { get;  }
        public Color Foreground { get;  }
        public Color Background { get;  }
        public bool Alert { get;  }

        public SearchOptions(string searchText, Color foreground, Color background, bool alert)
        {
            SearchText = searchText;
            Foreground = foreground;
            Background = background;
            Alert = alert;
        }

        private SearchOptions()
        {
        }
    }
}
