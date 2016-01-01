using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TailBlazer.Fixtures
{
    /// <summary>
    /// see http://www.superstarcoders.com/blogs/posts/recursive-select-in-c-sharp-and-linq.aspx
    /// </summary>
    public static class SelectManyExtensions
    {
        public static IEnumerable<T> SelectManyRecursive<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");

            T[] selectManyRecursive = source as T[] ?? source.ToArray();
            return !selectManyRecursive.Any()
                ? selectManyRecursive
                : selectManyRecursive.Concat(
                    selectManyRecursive
                        .SelectMany(i => selector(i).EmptyIfNull())
                        .SelectManyRecursive(selector)
                    );
        }

        public static IEnumerable<TSource> RecursiveSelect<TSource>(this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TSource>> childSelector)
        {
            return RecursiveSelect(source, childSelector, element => element);
        }

        public static IEnumerable<TResult> RecursiveSelect<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TSource>>
                childSelector,
            Func<TSource, TResult> selector)
        {
            return RecursiveSelect(source, childSelector, (element, index, depth) => selector(element));
        }

        public static IEnumerable<TResult> RecursiveSelect<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TSource>>
                childSelector,
            Func<TSource, int, TResult> selector)
        {
            return RecursiveSelect(source, childSelector, (element, index, depth) => selector(element, index));
        }

        public static IEnumerable<TResult> RecursiveSelect<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TSource>>
                childSelector,
            Func<TSource, int, int, TResult> selector)
        {
            return RecursiveSelect(source, childSelector, selector, 0);
        }

        private static IEnumerable<TResult> RecursiveSelect<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TSource>>
                childSelector,
            Func<TSource, int, int, TResult> selector,
            int depth)
        {
            return source.SelectMany((element, index) => Enumerable.Repeat(selector(element, index, depth), 1)
                .Concat(
                    RecursiveSelect(
                        childSelector(element) ??
                        Enumerable.Empty<TSource>(),
                        childSelector, selector, depth + 1)));
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }
    }

    public class StringMatchEnumerator : IEnumerable<MatchedString>
    {
        private readonly string _input;
        private readonly IEnumerable<string> _itemsToMatch;
        private readonly string _textToMatch;

        public StringMatchEnumerator(string input, string textToMatch)
        {
            if (textToMatch == null) throw new ArgumentNullException(nameof(textToMatch));
            _input = input;
            _textToMatch = textToMatch;
        }

        public StringMatchEnumerator(string input, IEnumerable<string> itemsToMatch)
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


            MatchedString[] latest;
            foreach (var textToMatch in _itemsToMatch)
            {

                latest = Yield(_input, textToMatch).ToArray();


                foreach (var child in latest)
                {
                    if (child.IsMatch)
                    {
                        yield return child;
                    }
                    else
                    {
                      //  yield return child;
                        foreach (var result in Yield(child.Part, textToMatch))
                        {
                            yield return result;
                        }
                    }

                }
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