using System;
using System.Collections.Generic;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    public struct LineIndex : IEquatable<LineIndex>
    {
        public int Line { get; }
        public int Index { get; }
        public long Start { get; }
        public long End { get; }
        public long Size => End - Start;
        public int Offset { get; }

        public LineIndexType Type { get; }
        public LineIndex(int line, int index, long startPosition, long endPosition)
        {
            Line = line;
            Index = index;
            Start = startPosition;
            End = endPosition;
            Offset = 0;
            Type= LineIndexType.Absolute;
        }

        public LineIndex(int line, int index, long startPosition, int offset)
        {
            Line = line;
            Index = index;
            Start = startPosition;
            End = -1;
            Offset = offset;
            Type= LineIndexType.Relative;
        }

        #region Comparison


        public static readonly IEqualityComparer<LineIndex> PositionComparer =
                                        Equality.CompareOn<LineIndex, long>(li => li.Start)
                                                .AndOn<LineIndex, long>(li => li.Offset);


        public static readonly IEqualityComparer<LineIndex> IndexComparer =Equality.CompareOn<LineIndex, int>(li => li.Index);
        private sealed class LineIndexEqualityComparer : IEqualityComparer<LineIndex>
        {
            public bool Equals(LineIndex x, LineIndex y)
            {
                return x.Index == y.Index && x.Start == y.Start && x.Offset == y.Offset && x.Type == y.Type;
            }

            public int GetHashCode(LineIndex obj)
            {
                unchecked
                {
                    var hashCode = obj.Index;
                    hashCode = (hashCode*397) ^ obj.Start.GetHashCode();
                    hashCode = (hashCode*397) ^ obj.Offset;
                    hashCode = (hashCode*397) ^ (int) obj.Type;
                    return hashCode;
                }
            }
        }

        private static readonly IEqualityComparer<LineIndex> LineIndexComparerInstance = new LineIndexEqualityComparer();

        public static IEqualityComparer<LineIndex> LineIndexComparer
        {
            get { return LineIndexComparerInstance; }
        }

        #endregion

        #region Equality

        public bool Equals(LineIndex other)
        {
            return Line == other.Line && Index == other.Index && Start == other.Start && End == other.End && Offset == other.Offset;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is LineIndex && Equals((LineIndex) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Line;
                hashCode = (hashCode*397) ^ Index;
                hashCode = (hashCode*397) ^ Start.GetHashCode();
                hashCode = (hashCode*397) ^ End.GetHashCode();
                hashCode = (hashCode*397) ^ Offset;
                return hashCode;
            }
        }

        public static bool operator ==(LineIndex left, LineIndex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LineIndex left, LineIndex right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            if (Type== LineIndexType.Relative)
                return $"{Index} ({Line}) {Start}+{Offset}";

            return $"{Index} ({Line}) {Start}->{End}, {Size}b";
        }
    }
}