using System;


namespace TailBlazer.Infrastucture
{
    public delegate void TextScrollDelegate(TextScrollInfo textScrollInfo);

    public class TextScrollInfo : IEquatable<TextScrollInfo>
    {
        public int FirstIndex { get;  }
        public int TotalChars { get;  }

        public TextScrollInfo(int firstIndex, int totalChars)
        {
            FirstIndex = firstIndex;
            TotalChars = totalChars;
        }

        #region Equality

        public bool Equals(TextScrollInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return FirstIndex == other.FirstIndex && TotalChars == other.TotalChars;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextScrollInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (FirstIndex*397) ^ TotalChars;
            }
        }

        public static bool operator ==(TextScrollInfo left, TextScrollInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TextScrollInfo left, TextScrollInfo right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"{FirstIndex} Take {TotalChars}";
        }
    }
}