using System;
using System.Collections.Generic;
using DynamicData.Binding;
using TailBlazer.Domain.FileHandling;

namespace TailBlazer.Views
{
    public class LineProxy: IComparable<LineProxy>, IComparable, IEquatable<LineProxy>
    {

        public static readonly IComparer<LineProxy> DefaultSort = SortExpressionComparer<LineProxy>
            .Ascending(p => p.Line.LineInfo.Start)
            .ThenByAscending(p => p.Line.LineInfo.Offset);

        public Line Line { get; }
        public long Number { get; }

        public int Index { get; }
        public string Text => Line.Text;
        public bool IsRecent { get; }

        public LineProxy(Line line)
        {
            Number = line.LineInfo.Start;
            Index = line.LineInfo.Index;
            Line = line;
            IsRecent = line.Timestamp.HasValue && DateTime.Now.Subtract(line.Timestamp.Value).TotalSeconds < 1;
        }



        public int CompareTo(LineProxy other)
        {
            return DefaultSort.Compare(this, other);
        }

        public int CompareTo(object obj)
        {
            return CompareTo((LineProxy) obj);
        }

        #region Equality

        public bool Equals(LineProxy other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Number == other.Number && Equals(Line, other.Line);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LineProxy) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Number.GetHashCode()*397) ^ (Line != null ? Line.GetHashCode() : 0);
            }
        }

        public static bool operator ==(LineProxy left, LineProxy right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LineProxy left, LineProxy right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"Line: {Line}";
        }
    }
}