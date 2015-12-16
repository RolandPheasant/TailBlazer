using System;

namespace TailBlazer.Domain.Settings
{
    public class State : IEquatable<State>
    {
        public int Version { get; }
        public string Value { get; }

        public State(int version, string value)
        {
            Version = version;
            Value = value;
        }

        #region Equality members

        public bool Equals(State other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((State) obj);
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        public static bool operator ==(State left, State right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(State left, State right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"Version: {Version}, Value: {Value}";
        }
    }
}
