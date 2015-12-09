
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

// ReSharper disable once CheckNamespace
namespace System
{


    public static class Extensions
    {

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static bool NextBoolean(this Random source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.NextDouble() > 0.5;
        }

        public static string FormatWithAbbreviation(this long source)
        {
            //TODO: not very efficient. Come back to this later
            //var powereof = Math.Floor(Math.Log10(source));
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = source;
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public static string Pluralise(this string source, int count)
        {
            return count == 1 ? $"{count} {source}" : $"{count} {source}s";
        }


    }
}

namespace System.Collections.Generic
{

    public static class Extensions
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

        public static string ToDelimited<T>(this IEnumerable<T> source, string delimiter=",")
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var array = source.AsArray();
            if (!array.Any())
                return string.Empty;
            return string.Join(string.Empty, array.WithDelimiter(delimiter));

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

        public static T[] AsArray<T>(this IEnumerable<T> source)
        {
           return source as T[] ?? source.ToArray();
        }

    }
}
