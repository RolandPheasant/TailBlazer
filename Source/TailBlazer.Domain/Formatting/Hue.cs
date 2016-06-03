using System;
using System.Windows.Media;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.Formatting
{
    public class Hue : IEquatable<Hue>
    {
        public string Swatch { get; }
        public string Name { get;  }
        public Color Foreground { get;  }
        public Color Background { get;  }

        public HueKey Key { get; }

        public Brush ForegroundBrush { get; }
        public Brush BackgroundBrush { get; }

        public static readonly Hue NotSpecified = new Hue();

        private Hue()
        {
            Swatch = "<None>";
            Name = "<None>";
            Key = new HueKey(Swatch, Name);
        }

        public Hue([NotNull] string swatch, [NotNull] string name, Color foreground, Color background)
        {
            if (swatch == null) throw new ArgumentNullException(nameof(swatch));
            if (name == null) throw new ArgumentNullException(nameof(name));

            Key = new HueKey(swatch,name);
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
            return Key.Equals(other.Key);
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
            return Key.GetHashCode();
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