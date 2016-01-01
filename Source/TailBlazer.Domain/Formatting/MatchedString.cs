using System;

namespace TailBlazer.Domain.Formatting
{
    public class MatchedString : IEquatable<MatchedString>
    {
        public string Part { get; }
        public bool IsMatch { get; }

        public MatchedString(string part, bool isMatch)
        {
            Part = part;
            IsMatch = isMatch;
        }
        

        #region Equality

        public bool Equals(MatchedString other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Part, other.Part) && IsMatch == other.IsMatch;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MatchedString)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Part?.GetHashCode() ?? 0) * 397) ^ IsMatch.GetHashCode();
            }
        }

        public static bool operator ==(MatchedString left, MatchedString right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MatchedString left, MatchedString right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"{Part}, ({IsMatch})";
        }
    }
}