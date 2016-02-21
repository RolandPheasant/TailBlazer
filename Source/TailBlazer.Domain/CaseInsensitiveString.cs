

using System;

namespace TailBlazer.Domain
{
    public struct CaseInsensitiveString : IEquatable<CaseInsensitiveString>
    {
        private readonly string _original;


        public CaseInsensitiveString(string source)
        {
            _original = source;
        }


        public static implicit operator string(CaseInsensitiveString source)
        {
            return source._original;
        }

        public static explicit operator CaseInsensitiveString(string source)
        {
            return new CaseInsensitiveString(source);
        }
        #region Equality

        public bool Equals(CaseInsensitiveString other)
        {
            return String.Equals(this, other, StringComparison.OrdinalIgnoreCase);

        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is CaseInsensitiveString && Equals((CaseInsensitiveString) obj);
        }

        public override int GetHashCode()
        {
            return _original.ToUpperInvariant().GetHashCode();
        }

        public static bool operator ==(CaseInsensitiveString left, CaseInsensitiveString right)
        {
            return String.Equals(left,right,StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator !=(CaseInsensitiveString left, CaseInsensitiveString right)
        {
            return !String.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        public override string ToString()
        {
            return _original;
        }
    }
}
