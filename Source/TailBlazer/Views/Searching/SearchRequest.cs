namespace TailBlazer.Views.Searching;

public class SearchRequest
{
    public string Text { get;  }
    public bool UseRegEx { get;  }

    public bool IsExclusion => Text.Substring(0, 1) == "-";

    public string TextWithoutExclusion => IsExclusion ? Text.Substring(1, Text.Length - 1) : Text;

    public SearchRequest(string text, bool useRegEx)
    {
        Text = text;
        UseRegEx = useRegEx;
    }
}