using System;

namespace TailBlazer.Domain.FileHandling
{
    /// <summary>
    /// Container which provides the latest tail changes
    /// </summary>
    public sealed class FileTailInfo : IEquatable<FileTailInfo>
    {
        public Line[] Lines { get; }
        public int Count => Lines.Length;
        public long Start { get; }
        public long End { get; }
        public long Size => End - Start;

        public DateTime DateTime { get; }

        public static readonly FileTailInfo Empty = new FileTailInfo(new Line[0]);

        public FileTailInfo(Line[] lines)
        {
            DateTime = DateTime.UtcNow;
            Lines = lines;
            Start = lines.Length == 0 ? 0 : lines[0].LineInfo.Start;
            End = lines.Length == 0 ? 0 : lines[lines.Length - 1].LineInfo.End;
        }

        #region Equality

        public bool Equals(FileTailInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Start == other.Start && End == other.End && DateTime.Equals(other.DateTime);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FileTailInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Start.GetHashCode();
                hashCode = (hashCode*397) ^ End.GetHashCode();
                hashCode = (hashCode*397) ^ DateTime.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(FileTailInfo left, FileTailInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FileTailInfo left, FileTailInfo right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"{nameof(Count)}: {Count}, {nameof(Start)}: {Start}, {nameof(End)}: {End}, {nameof(Size)}: {Size}";
        }
    }
}