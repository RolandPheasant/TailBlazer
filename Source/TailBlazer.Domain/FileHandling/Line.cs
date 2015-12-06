using System;

namespace TailBlazer.Domain.FileHandling
{
    public class Line : IEquatable<Line>
    {
        public int Number { get;  }
        public int Index { get; }
        public string Text { get;  }
        public DateTime? Timestamp { get;  }

        public LineInfo LineInfo { get;  }

        public Line(int number, string text)
        {
            Number = number;
            Text = text;
            Timestamp =  (DateTime?)null;
        }


        public Line(int number, string text, DateTime? timestamp)
        {
            Number = number;
            Text = text;
            Timestamp = timestamp;
        }


        public Line(LineInfo lineInfo, string text)
        {
            LineInfo = lineInfo;
            Text = text;
            Timestamp = lineInfo.EndOfTail ? DateTime.Now : (DateTime?)null;

            Number = LineInfo.Line;
            Index = LineInfo.Index;
        }

        #region Equality

        public bool Equals(Line other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Text, other.Text) && LineInfo.Equals(other.LineInfo);
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
                return ((Text != null ? Text.GetHashCode() : 0)*397) ^ LineInfo.GetHashCode();
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
