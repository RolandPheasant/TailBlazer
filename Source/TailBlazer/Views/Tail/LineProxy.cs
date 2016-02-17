using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Media;
using DynamicData.Binding;
using TailBlazer.Controls;
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
        public LineKey Key { get; }

        public IProperty<IEnumerable<DisplayText>> FormattedText { get; }

        public IProperty<SearchResultIndicatorStatus> IndicatorStatus { get; }

        public IProperty<LineMatchCollection> LineMatches { get; }

        public IProperty<Brush> IndicatorColour { get; }

        public bool IsRecent => Line.Timestamp.HasValue && DateTime.Now.Subtract(Line.Timestamp.Value).TotalSeconds < 0.25;


        public LineProxy([NotNull] Line line, 
            [NotNull] IObservable<IEnumerable<DisplayText>> formattedText,
            [NotNull] IObservable<LineMatchCollection> lineMatches)
        {
       
            if (line == null) throw new ArgumentNullException(nameof(line));
            if (formattedText == null) throw new ArgumentNullException(nameof(formattedText));
            if (lineMatches == null) throw new ArgumentNullException(nameof(lineMatches));

            Start = line.LineInfo.Start;
            Index = line.LineInfo.Index;
            Line = line;
            Key = Line.Key;

            var lineMatchesShared = lineMatches.Publish();

            FormattedText = formattedText.ForBinding();
            LineMatches = lineMatchesShared.ForBinding();

            IndicatorStatus = lineMatchesShared
                                .Select(lmc => CalculateStatus(lmc.FirstMatch))
                                .ForBinding();
            IndicatorColour = lineMatchesShared
                                .Select(lmc => lmc.FirstMatch?.Hue?.BackgroundBrush)
                                .ForBinding();

            _cleanUp = new CompositeDisposable(FormattedText, IndicatorStatus, LineMatches, IndicatorColour, lineMatchesShared.Connect());
        }
        
        private SearchResultIndicatorStatus CalculateStatus(LineMatch firstMatch)
        {
            if (firstMatch == null)
                return SearchResultIndicatorStatus.None;

            return firstMatch.UseRegex ? SearchResultIndicatorStatus.Regex : SearchResultIndicatorStatus.Text;
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
            return Key.Equals(other.Key);
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
            return Key.GetHashCode();
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