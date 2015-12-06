using System;

namespace TailBlazer.Domain.FileHandling
{
    public class Line : IEquatable<Line>
    {
        public int Number { get;  }
        public int Index { get; }
        public string Text { get;  }
        public DateTime? Timestamp { get;  }

        public LineIndex LineIndex { get;  }

        public Line(int number, string text, DateTime? timestamp)
        {
            Number = number;
            Text = text;
            Timestamp = timestamp;
        }


        public Line(LineIndex lineIndex, string text, DateTime? timestamp)
        {
            LineIndex = lineIndex;
            Text = text;
            Timestamp = timestamp;

            Number = LineIndex.Line;
            Index = LineIndex.Index;
        }

        #region Equality

        public bool Equals(Line other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Text, other.Text) && LineIndex.Equals(other.LineIndex);
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
                return ((Text != null ? Text.GetHashCode() : 0)*397) ^ LineIndex.GetHashCode();
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
