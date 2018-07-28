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
                    yield return new MatchedString(input);
                    yield break;
                }

                var matches = tomatch.RegEx.Value.Matches(input);
                
                if (matches.Count == 0)
                {
                    yield return new MatchedString(input);
                    yield break;
                }

                if (matches.Count > 4)
                {
                    yield return new MatchedString(input);
                    yield break;
                }

                var childMatches = new MatchedStringEnumerator2(input, matches.Cast<Match>()
                    .Select(match => match.Value)
                    .ToArray(),
                    tomatch);

                foreach (var item in childMatches)
                {
                    yield return item;
                }
            }
            else
            {
                foreach (var item in Yield2(input, tomatch))
                {
                    if (!item.IsMatch)
                    {
                        yield return new MatchedString(item.Part);
                    }
                    else
                    {
                        yield return new MatchedString(item.Part, tomatch);
                    }

                }
            }
        }

        private static IEnumerable<MatchedString> Yield2(string input, SearchMetadata meta)
        {
            var tomatch = meta.SearchText;
            var ignoreCase = meta.IgnoreCase;

            if (string.IsNullOrEmpty(input))
                yield break;

            string pattern = "(" + Regex.Escape(tomatch) + ")";
            var split = Regex.Split(input, pattern, ignoreCase ? CaseInsensitiveOptions : CaseSensitiveOptions);
            var length = split.Length;

            if (length == 0) yield break;

            if (length == 1)
            {
                yield return new MatchedString(input);
                yield break;
            }

            if (meta.Highlight == HighlightingMode.Line)
            {
                yield return new MatchedString(input, meta);
                yield break;
            }

            foreach (var item in split)
            {
                if (item.Equals(meta.SearchText, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new MatchedString(item, meta);
                }
                else
                {
                    yield return new MatchedString(item);
                }
            }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}