using System;

namespace TailBlazer.Domain
{
    public struct CaseInsensitiveString : IEquatable<CaseInsensitiveString>
    {
        private readonly string _original;
        private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;
        
        public CaseInsensitiveString(string source)
        {
            _original = source;
        }
        
        public static implicit operator string(CaseInsensitiveString source)
        {
            return source._original;
        }

        public static implicit operator CaseInsensitiveString(string source)
        {
            return new CaseInsensitiveString(source);
        }

        #region Equality

        public bool Equals(CaseInsensitiveString other)
        {
            return Comparer.Equals(this, other);

        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is CaseInsensitiveString && Equals((CaseInsensitiveString) obj);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
        }

        public static bool operator ==(CaseInsensitiveString left, CaseInsensitiveString right)
        {
            return Comparer.Equals(left, right);
        }

        public static bool operator !=(CaseInsensitiveString left, CaseInsensitiveString right)
        {
            return !Comparer.Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return _original;
        }
    }
}
