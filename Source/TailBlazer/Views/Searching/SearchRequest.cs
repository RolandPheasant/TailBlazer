namespace TailBlazer.Views.Searching
{
    public class SearchRequest
    {
        public string Text { get;  }
        public bool UseRegEx { get;  }

        public SearchRequest(string text, bool useRegEx)
        {
            Text = text;
            UseRegEx = useRegEx;
        }
    }
}