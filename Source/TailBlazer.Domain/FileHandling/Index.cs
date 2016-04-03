using System;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    public class Index : IEquatable<Index>
    {
        public long Start { get; }
        public long End { get; }
        public ImmutableList<long> Indicies { get; }
        public int Compression { get; }
        public int LineCount { get; }
        public int IndexCount => Indicies.Count;

        public IndexType Type { get; }

        public long Size => End - Start;

        public DateTime TimeStamp { get; } = DateTime.Now;

        public Index(long start, long end, long[] indicies, int compression, int lineCount, IndexType type)
        {
            Start = start;
            End = end;
            Indicies = new ImmutableList<long>(indicies);
            Compression = compression;
            LineCount = lineCount;
            Type = type;
        }
        

        public Index(long start, long end,  int compression, int lineCount, IndexType type)
        {
            Start = start;
            End = end;
            Indicies = new ImmutableList<long>();
            Compression = compression;
            LineCount = lineCount;
            Type = type;
        }

        public Index([NotNull] Index latest, Index previous)
        {
            if (latest == null) throw new ArgumentNullException(nameof(latest));
            if (previous == null) throw new ArgumentNullException(nameof(previous));

            Start = previous.Start;
            End = latest.End;
            Compression = latest.Compression;
            LineCount = latest.LineCount + previous.LineCount;
            Type = latest.Type;

            //combine latest arrays
            Indicies = previous.Indicies.Add(latest.Indicies);
        }

        #region Equality


        public bool Equals(Index other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Start == other.Start && End == other.End && Compression == other.Compression && LineCount == other.LineCount && Type == other.Type && TimeStamp.Equals(other.TimeStamp);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Index) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Start.GetHashCode();
                hashCode = (hashCode*397) ^ End.GetHashCode();
                hashCode = (hashCode*397) ^ Compression;
                hashCode = (hashCode*397) ^ LineCount;
                hashCode = (hashCode*397) ^ (int) Type;
                hashCode = (hashCode*397) ^ TimeStamp.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Index left, Index right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Index left, Index right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"{Type} {Start}->{End}  x{Compression} Compression. Lines: {LineCount}, Indicies: {IndexCount}, @ {TimeStamp}";
        }
    }
}