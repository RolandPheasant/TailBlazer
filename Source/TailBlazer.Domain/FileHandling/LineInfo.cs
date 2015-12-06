using System;
using System.Collections.Generic;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
 
    public struct LineInfo : IEquatable<LineInfo>
    {
        public int Line { get; }
        public int Index { get; }
        public long Start { get; }
        public long End { get; }
        public long Size => End - Start;
        public int Offset { get; }

        public bool EndOfTail { get; }

        public LineIndexType Type { get; }
        public LineInfo(int line, int index, long startPosition, long endPosition, bool endOfTail=false)
        {
            Line = line;
            Index = index;
            Start = startPosition;
            End = endPosition;
            Offset = 0;
            Type= LineIndexType.Absolute;
            EndOfTail = endOfTail;
        }

        public LineInfo(int line, int index, long startPosition, int offset, bool endOfTail=false)
        {
            Line = line;
            Index = index;
            Start = startPosition;
            End = -1;
            Offset = offset;
            Type= LineIndexType.Relative;
            EndOfTail = endOfTail;
        }

        #region Comparison


        public static readonly IEqualityComparer<LineInfo> PositionComparer =
                                        Equality.CompareOn<LineInfo, long>(li => li.Start)
                                                .AndOn<LineInfo, long>(li => li.Offset);


        public static readonly IEqualityComparer<LineInfo> FlexComparer = Equality.CompareOn<LineInfo, long>(li => li.Type== 
        LineIndexType.Absolute ? li.Start : li.Line);

        public static readonly IEqualityComparer<LineInfo> IndexComparer = Equality.CompareOn<LineInfo, int>(li => li.Index);

        private sealed class LineIndexEqualityComparer : IEqualityComparer<LineInfo>
        {
            public bool Equals(LineInfo x, LineInfo y)
            {
                return x.Index == y.Index && x.Start == y.Start && x.Offset == y.Offset && x.Type == y.Type;
            }

            public int GetHashCode(LineInfo obj)
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

        private static readonly IEqualityComparer<LineInfo> LineIndexComparerInstance = new LineIndexEqualityComparer();

        public static IEqualityComparer<LineInfo> LineIndexComparer
        {
            get { return LineIndexComparerInstance; }
        }

        #endregion

        #region Equality

        public bool Equals(LineInfo other)
        {
            //if (Type== LineIndexType.Absolute)
            //    return  Start == other.Start && Type == other.Type;

            return Line == other.Line && Index == other.Index && Start == other.Start && End == other.End && Offset == other.Offset && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is LineInfo && Equals((LineInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Line.GetHashCode();
                hashCode = (hashCode*397) ^ Index;
                hashCode = (hashCode*397) ^ Start.GetHashCode();
                hashCode = (hashCode*397) ^ End.GetHashCode();
                hashCode = (hashCode*397) ^ Offset;
                hashCode = (hashCode*397) ^ (int) Type;
                return hashCode;
            }
        }

        public static bool operator ==(LineInfo left, LineInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LineInfo left, LineInfo right)
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