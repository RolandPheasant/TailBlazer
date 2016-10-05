using System;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public static class FileTailInfoEx
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

    /// <summary>
    /// Container which provides the latest tail changes
    /// </summary>
    public sealed class TailInfo : IEquatable<TailInfo>
    {
        public Line[] Lines { get; }
        public int Count => Lines.Length;
        public long Start { get; }
        public long End { get; }
        public long Size => End - Start;

        public DateTime DateTime { get; }

        public static readonly TailInfo Empty = new TailInfo(new Line[0]);

        public TailInfo(Line[] lines)
        {
            DateTime = DateTime.UtcNow;
            Lines = lines;
            Start = lines.Length == 0 ? 0 : lines[0].LineInfo.Start;
            End = lines.Length == 0 ? 0 : lines[lines.Length - 1].LineInfo.End;
        }


        #region Equality

        public bool Equals(TailInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Start == other.Start && End == other.End ;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TailInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Start.GetHashCode();
                hashCode = (hashCode*397) ^ End.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(TailInfo left, TailInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TailInfo left, TailInfo right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"({Count} lines),  {Start}->{End}, {nameof(Size)}: {Size}";
        }
    }
}