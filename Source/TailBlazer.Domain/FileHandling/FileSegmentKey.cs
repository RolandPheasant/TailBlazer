using System;
using System.Collections.Generic;
using DynamicData.Binding;

namespace TailBlazer.Domain.FileHandling;

public struct FileSegmentKey : IEquatable<FileSegmentKey>, IComparable<FileSegmentKey>
{
    private readonly FileSegmentType _type;
    private readonly int _value;

    private static readonly IComparer<FileSegmentKey> DefaultOrder = SortExpressionComparer<FileSegmentKey>
        .Ascending(fsk => fsk._type == FileSegmentType.Head ? 1 : 2)
        .ThenByAscending(fsk => fsk._value);


    public static readonly FileSegmentKey Tail =new FileSegmentKey(-1, FileSegmentType.Tail);

    public FileSegmentKey(int index, FileSegmentType type)
    {
        _type = type;
        if (type == FileSegmentType.Tail)
        {
            _value = -1;
        }
        else
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            _value = index;
        }
    }

    #region Equality

    public static bool operator ==(FileSegmentKey left, FileSegmentKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FileSegmentKey left, FileSegmentKey right)
    {
        return !left.Equals(right);
    }

    public bool Equals(FileSegmentKey other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is FileSegmentKey && Equals((FileSegmentKey) obj);
    }

    public override int GetHashCode()
    {
        return _value;
    }

    #endregion

    public int CompareTo(FileSegmentKey other)
    {
        return DefaultOrder.Compare(this, other);
    }

    public override string ToString()
    {
        if (_type == FileSegmentType.Tail)
            return "Tail";
        return $"Head: {_value}";
    }
}