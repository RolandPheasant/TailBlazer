using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData.Kernel;

// ReSharper disable once CheckNamespace
namespace System.Collections
{
    public static class EnumerableEx
    {
        
        public static IEnumerable<T> YieldOne<T>(this T source)
        {
            yield return source;
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach (var item in source)
                action(item);
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            int i = 0;
            foreach (var item in source)
            {
                action(item,i);
                i++;
            }
        }
        public static IEnumerable<T> FromDelimited<T>(this string source, Func<string,T> converter, string delimiter = ",")
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            if (delimiter == null) throw new ArgumentNullException(nameof(delimiter));

            if (string.IsNullOrWhiteSpace(source))
                yield break;

            var strings =  source.Split(delimiter.ToCharArray());
            if (!strings.Any()) yield break;

            foreach (var s in strings)
                yield return converter(s);
        }

        public static string ToDelimited<T>(this IEnumerable<T> source, string delimiter=",")
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (delimiter == null) throw new ArgumentNullException(nameof(delimiter));

            var array = source.AsArray();
            return !array.Any() ? string.Empty : string.Join(string.Empty, array.WithDelimiter(delimiter));
        }

        public static IEnumerable<string>  WithDelimiter<T>(this IEnumerable<T> source, string delimiter)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var array = source.AsArray();
            if (!array.Any()) yield return string.Empty;

            yield return array.Select(t => t.ToString()).First();

            foreach (var item in array.Skip(1))
                yield return $"{delimiter}{item}";

        }

        //public static T[] AsArray<T>(this IEnumerable<T> source)
        //{
        //    return source as T[] ?? source.ToArray();
        //}

    }
}