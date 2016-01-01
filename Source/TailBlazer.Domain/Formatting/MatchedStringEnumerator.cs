using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DynamicData.Kernel;

namespace TailBlazer.Domain.Formatting
{

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
                yield break;
            }

            var strings = _itemsToMatch.AsArray();
            MatchedString[] matches = new MatchedString[0];
            for (int i = 0; i < strings.Length; i++)
            {
                var stringToMatch = strings[i];
                if (i == 0)
                {
                    matches = Yield(_input, stringToMatch).ToArray();
                }
                else
                {
                    matches = matches.SelectMany(ms => ms.IsMatch ? new[] { ms } : Yield(ms.Part, stringToMatch)).ToArray();
                }
            }
            foreach (var matchedString in matches)
            {
                yield return matchedString;
            }

        }


        private  static IEnumerable<MatchedString> Yield(string input, string tomatch)
        {
            var split = input.Split(new[] { tomatch }, StringSplitOptions.None);
            var length = split.Length;

            if (length == 0)
            {
                yield break;
            }

            if (length == 1)
            {
                yield return new MatchedString(input, false);
                yield break;
            }
            
            for (int i = 0; i < split.Length; i++)
            {
                var current = split[i];

                if (string.IsNullOrEmpty(current))
                {

                    yield return new MatchedString(tomatch, true);
                }
                else if (i > 0 && !string.IsNullOrEmpty(split[i - 1]))
                {
                    yield return new MatchedString(tomatch, true);
                    yield return new MatchedString(current, false);

                }
                else
                {
                    yield return new MatchedString(current, false);
                }
            }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}