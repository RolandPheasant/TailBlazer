using System;

namespace TailBlazer.Domain.FileHandling
{
    public struct FileSegmentKey : IEquatable<FileSegmentKey>
    {
        private readonly FileSegmentType _type;
        private readonly int _value;

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

        public override string ToString()
        {
            if (_type == FileSegmentType.Tail)
                return "Tail";
            return $"Head: {_value}";
        }
    }
}