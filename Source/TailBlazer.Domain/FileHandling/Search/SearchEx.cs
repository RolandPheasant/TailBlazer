using System;
using System.Text.RegularExpressions;

namespace TailBlazer.Domain.FileHandling.Search
{
    public static class SearchEx
    {
        public static Func<string, bool> BuildPredicate(this SearchMetadata source)
        {
            Func<string, bool> predicate;
            if (!source.UseRegex)
            {
                predicate = s => s.Contains(source.SearchText, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                var regex = new Regex(source.SearchText);
                predicate = s => regex.IsMatch(s);
            }
            return predicate;
        } 
    }
}