using System;
using System.Collections.Generic;
using System.Drawing.Text;

namespace TailBlazer.Domain.FileHandling
{
    public struct LineKey : IEquatable<LineKey>
    {
        private readonly string _text;
        private readonly long _start;

        public LineKey(string text, long start)
        {
            _text = text;
            _start = start;
        }

        #region Equality

        public bool Equals(LineKey other)
        {
            return string.Equals(_text, other._text) && _start == other._start;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is LineKey && Equals((LineKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_text != null ? _text.GetHashCode() : 0)*397) ^ _start.GetHashCode();
            }
        }

        public static bool operator ==(LineKey left, LineKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LineKey left, LineKey right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return $"{_text} (@{_start})";
        }
    }

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
            Text = text;
            Timestamp = timestamp;
            Key = new LineKey(text, number);
        }

   


        public Line(LineInfo lineInfo, string text, DateTime? timestamp)
        {

            LineInfo = lineInfo;
            Text = text;
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
