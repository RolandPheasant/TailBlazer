using System;
using System.Collections.Generic;
using System.Linq;
using TailBlazer.Domain.FileHandling;

namespace TailBlazer.Fixtures
{
    internal static class LinesEx
    {
        public static string Join(this IEnumerable<string> lines)
        {
            return string.Join(Environment.NewLine, lines);
        }

        public static string Join(this IEnumerable<Line> lines)
        {
            return string.Join(Environment.NewLine, lines.Select(l=>l.Text));
        }
    }
}