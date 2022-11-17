using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DynamicData.Kernel;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Domain.Formatting;

public class MatchedStringEnumerator2 : IEnumerable<MatchedString>
{
    private readonly string _input;
    private readonly IEnumerable<string> _itemsToMatch;
    private readonly SearchMetadata _tomatch;

    public MatchedStringEnumerator2(string input, IEnumerable<string> itemsToMatch, SearchMetadata tomatch)
    {
        _input = input;
        _itemsToMatch = itemsToMatch;
        _tomatch = tomatch;
    }

    public IEnumerator<MatchedString> GetEnumerator()
    {

        var strings = _itemsToMatch.AsArray();
        var matches = new MatchedString[0];
        for (int i = 0; i < strings.Length; i++)
        {
            var stringToMatch = strings[i];
            if (i == 0)
            {
                matches = Yield(_input, stringToMatch).ToArray();
            }
            else
            {
                matches = matches.SelectMany(ms => ms.IsMatch
                    ? new[] {ms}
                    : Yield(ms.Part, stringToMatch)).ToArray();
            }
        }
        foreach (var matchedString in matches)
        {
            yield return matchedString;
        }
    }

    private  IEnumerable<MatchedString> Yield(string input, string tomatch)
    {

        if (string.IsNullOrEmpty(input))
            yield break;

        string pattern = "(" + Regex.Escape(tomatch) + ")";
        var split = Regex.Split(input, pattern, RegexOptions.IgnoreCase);
        var length = split.Length;

        if (length == 0) yield break;

        if (length == 1)
        {
            yield return new MatchedString(input);
            yield break;
        }

        foreach (var item in split)
        {
            if (item.Equals(tomatch, StringComparison.OrdinalIgnoreCase))
            {
                yield return new MatchedString(item, _tomatch);
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

public class MatchedStringEnumerator : IEnumerable<MatchedString>
{
    private readonly string _input;
    private readonly IEnumerable<string> _itemsToMatch;
    private readonly string _textToMatch;

    public MatchedStringEnumerator(string input, string textToMatch)
    {
        if (textToMatch == null) throw new ArgumentNullException(nameof(textToMatch));
        _input = input;
        _textToMatch = textToMatch;
    }

    public MatchedStringEnumerator(string input, IEnumerable<string> itemsToMatch)
    {
        _input = input;
        _itemsToMatch = itemsToMatch;
    }

    public IEnumerator<MatchedString> GetEnumerator()
    {

        if (_textToMatch != null)
        {
            foreach (var result in Yield(_input, _textToMatch))
            {
                yield return result;
            }
        }
        else
        {
            var strings = _itemsToMatch.AsArray();
            var matches = Array.Empty<MatchedString>();
                
            for (int i = 0; i < strings.Length; i++)
            {
                var stringToMatch = strings[i];
                if (i == 0)
                {
                    matches = Yield(_input, stringToMatch).ToArray();
                }
                else
                {
                    matches = matches.SelectMany(ms => ms.IsMatch
                        ? new[] { ms }
                        : Yield(ms.Part, stringToMatch)).ToArray();
                }
            }
            foreach (var matchedString in matches)
            {
                yield return matchedString;
            }
        }

    }

    private static IEnumerable<MatchedString> Yield(string input, string tomatch)
    {

        if (string.IsNullOrEmpty(input))
            yield break;


        //TODO: Check whether there are perf-issues with RegEx
        var split = Regex.Split(input, tomatch, RegexOptions.IgnoreCase);

        var length = split.Length;

        if (length == 0) yield break;

        if (length == 1)
        {
            yield return new MatchedString(input);
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
                yield return new MatchedString(current);

                currentLength = current.Length + currentLength + tomatch.Length;
            }
            else
            {
                yield return new MatchedString(current);
                currentLength = current.Length + currentLength;
            }

        }
    }



    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}