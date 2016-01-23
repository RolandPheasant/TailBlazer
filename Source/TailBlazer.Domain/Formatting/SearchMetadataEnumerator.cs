using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DynamicData.Kernel;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Domain.Formatting
{
    public class SearchMetadataEnumerator : IEnumerable<MatchedString>
    {
        private readonly string _input;
        private readonly IEnumerable<SearchMetadata> _itemsToMatch;

        public SearchMetadataEnumerator(string input, IEnumerable<SearchMetadata> itemsToMatch)
        {
            _input = input;
            _itemsToMatch = itemsToMatch;
        }

        public IEnumerator<MatchedString> GetEnumerator()
        {
            var strings = _itemsToMatch.AsArray();
            var matches = new MatchedString[0];
            for (int i = 0; i < strings.Length; i++)
            {
                var searchMetadata = strings[i];
                if (i == 0)
                {
                    matches = Yield(_input, searchMetadata).ToArray();
                }
                else
                {
                    matches = matches.SelectMany(ms => ms.IsMatch
                        ? new[] { ms }
                        : Yield(ms.Part, searchMetadata)).ToArray();
                }
            }

            return ((IEnumerable<MatchedString>) matches).GetEnumerator();

        }

        private const RegexOptions CaseInsensitiveOptions = RegexOptions.IgnorePatternWhitespace
                                                            | RegexOptions.Compiled
                                                            | RegexOptions.IgnoreCase;

        private const RegexOptions CaseSensitiveOptions = RegexOptions.IgnorePatternWhitespace
                                                          | RegexOptions.Compiled;

        private static IEnumerable<MatchedString> Yield(string input, SearchMetadata tomatch)
        {

            if (string.IsNullOrEmpty(input)) yield break;


            if (tomatch.UseRegex)
            {

                if (!tomatch.RegEx.HasValue || !tomatch.SearchText.IsLongerThanOrEqualTo(3))
                {
                    yield return new MatchedString(input, false);
                    yield break;
                }

                var matches = tomatch.RegEx.Value.Matches(input);
                
                if (matches.Count == 0)
                {
                    yield return new MatchedString(input, false);
                    yield break;
                }

                if (matches.Count > 4)
                {
                    yield return new MatchedString(input, false, true);
                    yield break;
                }

                var childMatches = new MatchedStringEnumerator(input, matches.Cast<Match>()
                    .Select(match => match.Value)
                    .ToArray());

                foreach (var item in childMatches)
                {
                    yield return item;
                }
            }
            else
            {

                foreach (var item in Yield(input, tomatch.SearchText, tomatch.IgnoreCase))
                {
                    //yield return item;
                    yield return new MatchedString(item.Part,item.IsMatch);
                }
            }
        }

        private static IEnumerable<MatchedString> Yield(string input, string tomatch, bool ignoreCase)
        {

            if (string.IsNullOrEmpty(input))
                yield break;


            //TODO: Check whether there are perf-issues with RegEx
            var split = Regex.Split(input, tomatch, ignoreCase ? CaseInsensitiveOptions : CaseSensitiveOptions);

            var length = split.Length;

            if (length == 0) yield break;

            if (length == 1)
            {
                yield return new MatchedString(input, false);
                yield break;
            }

            //  int start =0;
            int currentLength = 0;

            for (int i = 0; i < split.Length; i++)
            {
                var current = split[i] ?? string.Empty;

                if (string.IsNullOrEmpty(current))
                {
                    //Get original string back as the user may have searched in a different case
                    var originalString = input.Substring(currentLength, tomatch.Length);
                    yield return new MatchedString(originalString, true);

                    currentLength = current.Length + currentLength + tomatch.Length;
                    if (currentLength + tomatch.Length > input.Length)
                        yield break;
                }
                else if (i > 0 && !string.IsNullOrEmpty(split[i - 1]))
                {
                    if (currentLength + tomatch.Length > input.Length)
                        yield break;

                    //Get original string back as the user may have searched in a different case
                    var originalString = input.Substring(currentLength, tomatch.Length);


                    yield return new MatchedString(originalString, true);
                    yield return new MatchedString(current, false);

                    currentLength = current.Length + currentLength + tomatch.Length;
                }
                else
                {
                    yield return new MatchedString(current, false);
                    currentLength = current.Length + currentLength;
                }

            }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}