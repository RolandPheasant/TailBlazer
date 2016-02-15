using System;
using System.Windows.Media;

namespace TailBlazer.Domain.Formatting
{
    public class Hue : IEquatable<Hue>
    {
        public Color Foreground { get;  }
        public Color Background { get;  }

        public Hue(Color foreground, Color background)
        {
            Foreground = foreground;
            Background = background;
        }

        #region Equality

        public bool Equals(Hue other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Foreground.Equals(other.Foreground) && Background.Equals(other.Background);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Hue) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Foreground.GetHashCode()*397) ^ Background.GetHashCode();
            }
        }

        public static bool operator ==(Hue left, Hue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Hue left, Hue right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}