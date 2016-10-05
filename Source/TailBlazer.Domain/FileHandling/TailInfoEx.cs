using System;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public static class TailInfoEx
    {
        public static TailInfo Trim([NotNull] this TailInfo source, int size)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            if (size >= source.Count)
                return source;
            var trimmed = new Line[size];
            Array.Copy(source.Lines, source.Count-size, trimmed, 0, size);
            return new TailInfo(trimmed);
        }
    }
}