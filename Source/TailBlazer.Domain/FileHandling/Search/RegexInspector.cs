using System;
using System.Text.RegularExpressions;

namespace TailBlazer.Domain.FileHandling.Search;

public class RegexInspector
{

    private readonly Regex _isPlainText;

    public RegexInspector()
    {
        _isPlainText = new Regex("^[a-zA-Z0-9 ]*$", RegexOptions.Compiled);
    }

    public bool DoesThisLookLikeRegEx(string text)
    {
        var withNegation = text.WithNegation();
        return !string.IsNullOrEmpty(text) && !_isPlainText.IsMatch(withNegation.Text);
    }
}