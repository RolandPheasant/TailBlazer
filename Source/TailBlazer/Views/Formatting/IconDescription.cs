using System;
using Humanizer;
using MaterialDesignThemes.Wpf;

namespace TailBlazer.Views.Searching
{
    public class IconDescription : IEquatable<IconDescription>
    {
        public string Name { get; }
        public PackIconKind Type { get; }

        public string Description { get; }

        public IconDescription(PackIconKind type, string name)
        {
            Type = type;
            Name = name;
            Description = name.Humanize();
        }

      

        #region Equality

        public bool Equals(IconDescription other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((IconDescription) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0)*397) ^ (int) Type;
            }
        }

        public static bool operator ==(IconDescription left, IconDescription right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(IconDescription left, IconDescription right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return Name;
        }
    }
}