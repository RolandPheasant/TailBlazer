namespace TailBlazer.Domain.FileHandling;

public struct LineKey : IEquatable<LineKey>
{
    private readonly string _text;
    private readonly long _start;

    public LineKey(string text, long start)
    {
        _text = text;
        _start = start;
    }

    #region Equality

    public bool Equals(LineKey other)
    {
        return string.Equals(_text, other._text) && _start == other._start;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is LineKey && Equals((LineKey) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((_text?.GetHashCode() ?? 0)*397) ^ _start.GetHashCode();
        }
    }

    public static bool operator ==(LineKey left, LineKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LineKey left, LineKey right)
    {
        return !left.Equals(right);
    }

    #endregion

    public override string ToString()
    {
        return $"{_text} (@{_start})";
    }
}