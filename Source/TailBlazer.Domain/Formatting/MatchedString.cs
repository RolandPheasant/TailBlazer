using System;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Domain.Formatting
{
    public class MatchedString : IEquatable<MatchedString>
    {
        private readonly SearchMetadata _metadata;
        public string Part { get; }

        public bool IsMatch { get; }

        public Hue Hue => _metadata?.HighlightHue;

        public Hue BackgroundHue { get; } = null;

        public MatchedString(string part)
        {
            Part = part;
            IsMatch = false;
        }

        public MatchedString(string part, bool isMatch)
        {
            Part = part;
            IsMatch = isMatch;
        }

        public MatchedString(string part, SearchMetadata metadata)
        {
            _metadata = metadata;
            Part = part;
            IsMatch = true;
            BackgroundHue = metadata.Highlight == HighlightingMode.Line ? metadata.HighlightHue : null;
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
            return Equals((MatchedString) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Part?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ IsMatch.GetHashCode();
                return hashCode;
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