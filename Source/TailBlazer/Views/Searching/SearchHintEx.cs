using System;
using System.Text.RegularExpressions;

namespace TailBlazer.Views.Searching
{
    public static class SearchHintEx
    {
        public static SearchHintMessage BuildMessage(this SearchRequest source)
        {
            if (string.IsNullOrEmpty(source.Text))
                return new SearchHintMessage(true, $"Type to search using {(source.UseRegEx ? "regex" : "plain text")}");

            if (string.IsNullOrEmpty(source.Text))
                return SearchHintMessage.Valid;

            if (source.UseRegEx && !source.Text.IsLongerThanOrEqualTo(2))
                return new SearchHintMessage(false, "Regex must be at least 2 characters");

            if (!source.UseRegEx && !source.Text.IsLongerThanOrEqualTo(3))
                return new SearchHintMessage(false, "Text must be at least 3 characters");

            if ((!source.UseRegEx && source.Text.Contains(@"\")) ||
                (source.Text.Trim().Length == 0))
                return new SearchHintMessage(false, "Text contains illegal characters");

            try
            {
                var test = new Regex(source.Text);
            }
            catch (Exception)
            {
                return new SearchHintMessage(false, "Invalid regular expression");
            }
            
            var message = $"Hit enter to search using {(source.UseRegEx ? "regex" : "plain text")}";

            return new SearchHintMessage(true, message);
        }
    }
}