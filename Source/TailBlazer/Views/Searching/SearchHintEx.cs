using System;
using System.Text.RegularExpressions;

namespace TailBlazer.Views.Searching;

public static class SearchHintEx
{
    public static SearchHintMessage BuildMessage(this SearchRequest source)
    {
        if (string.IsNullOrEmpty(source.Text))
            return new SearchHintMessage(true, $"Type to search using {(source.UseRegEx ? "regex" : "plain text")}");

        if (source.UseRegEx && !source.Text.IsLongerThanOrEqualTo(2))
            return new SearchHintMessage(false, "Regex must be at least 2 characters");

        var textLength = source.IsExclusion ? 4 : 3;
        if (!source.UseRegEx && !source.Text.IsLongerThanOrEqualTo(textLength))
            return new SearchHintMessage(false, "Text must be at least 3 characters");

        if ((!source.UseRegEx && source.Text.Contains(@"\")) || (source.TextWithoutExclusion.Trim().Length == 0))
            return new SearchHintMessage(false, "Text contains illegal characters");

        if (source.UseRegEx)
        {
            try
            {
                var test = new Regex(source.Text);
            }
            catch (Exception)
            {
                return new SearchHintMessage(false, "Invalid regular expression");
            }
        }
            
        var message = $"Hit enter to search using {(source.UseRegEx ? "regex" : "plain text")}";

        return new SearchHintMessage(true, message);
    }
}