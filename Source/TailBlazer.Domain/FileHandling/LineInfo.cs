namespace TailBlazer.Domain.FileHandling;

public struct LineInfo : IEquatable<LineInfo>
{
    public int Line { get; }
    public int Index { get; }
    public long Start { get; }
    public int Offset { get; }

    private LineIndexType Type { get; }
    private long End { get; }
    private long Size => End - Start;

    public LineInfo(int line, int index, long startPosition, long endPosition)
    {
        Line = line;
        Index = index;
        Start = startPosition;
        End = endPosition;
        Offset = 0;
        Type= LineIndexType.Absolute;
    }

    #region Equality

    public bool Equals(LineInfo other)
    {
        //if (Type== LineIndexType.Absolute)
        //    return  Start == other.Start && Type == other.Type;

        return Line == other.Line && Index == other.Index && Start == other.Start && End == other.End && Offset == other.Offset && Type == other.Type;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        return obj is LineInfo info && Equals(info);
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