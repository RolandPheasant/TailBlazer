using System;
using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling
{
    public class Line : IEquatable<Line>
    {
        public int Number { get;  }
        public int Index { get; }
        public string Text { get;  }
        public DateTime? Timestamp { get;  }
        public LineInfo LineInfo { get;  }
        private long Start { get; }
        public LineKey Key { get; }

        [Obsolete("Only used for testing")]
        public Line(int number, string text, DateTime? timestamp)
        {
            Number = number;
            Start = number;
            Text = text ?? string.Empty;
            Timestamp = timestamp;
            Key = new LineKey(text, number);
        }

   
        public Line(LineInfo lineInfo, string text, DateTime? timestamp)
        {
            LineInfo = lineInfo;
            Text = text ?? string.Empty;
            Timestamp = timestamp;
            Number = LineInfo.Line;
            Start = LineInfo.Start;
            Index = LineInfo.Index;
            Key = new LineKey(text, lineInfo.Start);
        }

        private sealed class TextStartEqualityComparer : IEqualityComparer<Line>
        {
            public bool Equals(Line x, Line y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.Text, y.Text) && x.Start == y.Start;
            }

            public int GetHashCode(Line obj)
            {
                unchecked
                {
                    return ((obj.Text?.GetHashCode() ?? 0)*397) ^ obj.Start.GetHashCode();
                }
            }
        }

        public static IEqualityComparer<Line> TextStartComparer { get; } = new TextStartEqualityComparer();

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
                return ((Text?.GetHashCode() ?? 0)*397) ^ LineInfo.GetHashCode();
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
