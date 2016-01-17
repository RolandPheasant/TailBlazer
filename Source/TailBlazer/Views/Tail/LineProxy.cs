using System;
using System.Collections.Generic;
using DynamicData.Binding;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Views.Tail
{
    public class LineProxy: AbstractNotifyPropertyChanged,IComparable<LineProxy>, IComparable, IEquatable<LineProxy>, IDisposable
    {
        private readonly IDisposable _cleanUp;

        public static readonly IComparer<LineProxy> DefaultSort = SortExpressionComparer<LineProxy>
            .Ascending(p => p.Line.LineInfo.Start)
            .ThenByAscending(p => p.Line.LineInfo.Offset);

        public Line Line { get; }
        public long Start { get; }
        public int Index { get; }
        public string Text => Line.Text;

        public  IProperty<IEnumerable<DisplayText>> FormattedText { get; }

        public bool IsRecent => Line.Timestamp.HasValue && DateTime.Now.Subtract(Line.Timestamp.Value).TotalSeconds < 0.25;

        public LineProxy([NotNull] Line line, [NotNull] IObservable<IEnumerable<DisplayText>> formattedText)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            if (formattedText == null) throw new ArgumentNullException(nameof(formattedText));
            Start = line.LineInfo.Start;
            Index = line.LineInfo.Index;
            Line = line;
    

            FormattedText = formattedText.ForBinding();
            _cleanUp = FormattedText;
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
            return Start == other.Start && Equals(Line, other.Line);
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
                return (Start.GetHashCode()*397) ^ (Line?.GetHashCode() ?? 0);
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

        public void Dispose()
        {
            _cleanUp.Dispose();
        }


        public override string ToString()
        {
            return $"{Line}";
        }
    }
}