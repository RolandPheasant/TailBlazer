using System;

namespace TailBlazer.Domain.FileHandling
{
    public struct LineIndex : IEquatable<LineIndex>
    {
        private readonly int _line;
        private readonly int _index;
        private readonly long _startPosition;
        private readonly long _endPosition;

        public LineIndex(int line, int index, long startPosition, long endPosition)
        {
            _line = line;
            _index = index;
            _startPosition = startPosition;
            _endPosition = endPosition;
        }

        public int Line => _line;

        public int Index => _index;

        public long Start => _startPosition;

        public long End => _endPosition;

        public long Size => _endPosition - _startPosition;



        #region Equality

        public bool Equals(LineIndex other)
        {
            return _line == other._line && _index == other._index;
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
                return (_line*397) ^ _index;
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
            return $"{Line} (index={Index})";
        }
    }
}