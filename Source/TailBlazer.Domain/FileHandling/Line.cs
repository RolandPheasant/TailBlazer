using System;

namespace TailBlazer.Domain.FileHandling
{
    public class Line : IEquatable<Line>
    {
        public int Number { get;  }
        public string Text { get;  }

        public Line(int number, string text)
        {
            Number = number;
            Text = text;
        }

        #region Equality

        public bool Equals(Line other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Number == other.Number && string.Equals(Text, other.Text);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Line) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Number*397) ^ (Text?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(Line left, Line right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Line left, Line right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"{Number}: {Text}";
        }
    }
}
