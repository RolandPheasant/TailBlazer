using System;
using System.IO;

namespace TailBlazer.Domain.FileHandling.Recent
{
    public class RecentFile : IEquatable<RecentFile>
    {
        public DateTime Timestamp { get; }
        public string  Name  { get; }

        public RecentFile(FileInfo fileInfo)
        {
            Name = fileInfo.FullName;
            Timestamp = DateTime.UtcNow;
        }

        public RecentFile(DateTime timestamp, string name)
        {
            Timestamp = timestamp;
            Name = name;
        }

        #region Equality

        public bool Equals(RecentFile other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Timestamp.Equals(other.Timestamp) && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RecentFile) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Timestamp.GetHashCode()*397) ^ (Name?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(RecentFile left, RecentFile right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RecentFile left, RecentFile right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"{Name} ({Timestamp})";
        }
    }
}