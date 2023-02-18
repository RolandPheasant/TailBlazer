using System.Text.RegularExpressions;
using DynamicData.Kernel;

namespace TailBlazer.Domain.FileHandling.Search;

public static class SearchEx
{
    public static Func<string, bool> BuildPredicate(this SearchMetadata source)
    {
        Func<string, bool> predicate;
        if (!source.UseRegex)
        {
            var stringComparison = source.StringComparison;
            predicate = s => !string.IsNullOrEmpty(s) && s.Contains(source.SearchText, stringComparison);
        }
        else
        {
            var options = source.RegexOptions;
            var regex = new Regex(source.SearchText, options);
            predicate = s => regex.IsMatch(s);
        }
        return predicate;
    }

    public static Optional<Regex> BuildRegEx(this SearchMetadata source)
    {
        if (source.UseRegex)
        {
            var options = source.RegexOptions;
            return new Regex(source.SearchText, options);
        }
        return Optional<Regex>.None;
    }
}