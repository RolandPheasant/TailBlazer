using System.Collections.Generic;

namespace TailBlazer.Views.Formatting
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
    }
}