using System.Collections.Generic;
using System.Linq;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Domain.Formatting
{
    public static class MatchedStringEx
    { 
        public static IEnumerable<MatchedString> MatchString(this string source, string textToMatch)
        {
            return new MatchedStringEnumerator(source, textToMatch);
        }
        public static IEnumerable<MatchedString> MatchString(this string source, IEnumerable<string> itemsToMatch)
        {
            return new MatchedStringEnumerator(source, itemsToMatch);
        }

        public static IEnumerable<MatchedString> MatchString(this string source, IEnumerable<SearchMetadata> itemsToMatch)
        {
            //Doctor this to allow ReEx matching
            return new MatchedStringEnumerator(source, itemsToMatch.Select(m=>m.SearchText));
        }
        
        //public static string SafeSubstring(this string source, IEnumerable<string> itemsToMatch)
        //{
        //    return new MatchedStringEnumerator(source, itemsToMatch);
        //}
    }
}