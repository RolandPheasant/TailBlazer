using System;
using System.Windows.Media;

namespace TailBlazer.Domain.Formatting
{
    public class Hue : IEquatable<Hue>
    {
        public string Swatch { get; set; }
        public string Name { get; set; }
        public Color Foreground { get;  }
        public Color Background { get;  }

        public Brush ForegroundBrush { get; }
        public Brush BackgroundBrush { get; }
        
        public Hue(string swatch, string name, Color foreground, Color background)
        {
            Swatch = swatch;
            Name = name;
            Foreground = foreground;
            Background = background;

            ForegroundBrush = new SolidColorBrush(foreground);
            ForegroundBrush.Freeze();


            BackgroundBrush = new SolidColorBrush(background);
            BackgroundBrush.Freeze();

        }
        
        #region Equality

        public bool Equals(Hue other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Swatch, other.Swatch) && string.Equals(Name, other.Name);
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
                return ((Swatch != null ? Swatch.GetHashCode() : 0)*397) ^ (Name != null ? Name.GetHashCode() : 0);
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

        public override string ToString()
        {
            return $"{Swatch} ({Name})";
        }
    }
}