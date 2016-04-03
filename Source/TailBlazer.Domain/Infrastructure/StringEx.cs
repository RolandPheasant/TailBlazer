
// ReSharper disable once CheckNamespace
namespace System
{
    public static class StringEx
    {
        public static bool IsLongerThan(this string source, int length)
        {
            return !string.IsNullOrEmpty(source) && source.Length > length;
        }

        public static bool IsLongerThanOrEqualTo(this string source, int length)
        {
            return !string.IsNullOrEmpty(source) && source.Length >= length;
        }

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
}
