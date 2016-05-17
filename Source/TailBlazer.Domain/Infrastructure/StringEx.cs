
using System.Collections.Generic;
using System.Reactive.Linq;

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

        //// Based on http://stackoverflow.com/a/11124118/619960
        //// Credit to user humbads 
        public static string FormatWithAbbreviation(this long source)
        {
            // Get absolute value
            long absolute_i = (source < 0 ? -source : source);

            // Determine the suffix and readable value
            string suffix;
            double len;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                len = (source >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                len = (source >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                len = (source >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                len = (source >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                len = (source >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                len = source;
            }
            else
            {
                return source.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            len = (len / 1024);
            // Return formatted number with suffix
            return $"{len:0.##} {suffix}";

        }

        public static string Pluralise(this string source, int count)
        {
            return count == 1 ? $"{count} {source}" : $"{count} {source}s";
        }


    }
}
