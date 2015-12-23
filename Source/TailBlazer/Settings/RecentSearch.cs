using System;

namespace TailBlazer.Settings
{
    public class RecentSearch : IEquatable<RecentSearch>
    {
        public DateTime Timestamp { get; }
        public string  Text  { get; }

        public RecentSearch(string seaarchText)
        {
            Text = seaarchText;
            Timestamp = DateTime.Now;
        }

        public RecentSearch(DateTime timestamp, string text)
        {
            Timestamp = timestamp;
            Text = text;
        }

        #region Equality

        public bool Equals(RecentSearch other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Timestamp.Equals(other.Timestamp) && string.Equals(Text, other.Text);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RecentSearch) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Timestamp.GetHashCode()*397) ^ (Text?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(RecentSearch left, RecentSearch right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RecentSearch left, RecentSearch right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"{Text} ({Timestamp})";
        }
    }
}