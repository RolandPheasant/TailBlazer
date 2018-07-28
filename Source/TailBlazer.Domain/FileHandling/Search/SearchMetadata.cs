using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DynamicData.Kernel;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Domain.FileHandling.Search
{

    public class SearchMetadata : IEquatable<SearchMetadata>
    {
        public int Position { get;  }
        public string SearchText { get; }
        public bool Filter { get; }
        public HighlightingMode Highlight { get; }
        public bool UseRegex { get; }
        public bool IgnoreCase { get; }
        public Optional<Regex> RegEx { get; }
        public Func<string, bool> Predicate { get; }
        public Hue HighlightHue { get; }
        public string IconKind { get; }
        public bool IsGlobal { get;  }
        public bool IsExclusion { get;  }

        public SearchMetadata([NotNull] SearchMetadata searchMetadata, int newPosition)
        {
            if (searchMetadata == null) throw new ArgumentNullException(nameof(searchMetadata));

            Position = newPosition;
            SearchText = searchMetadata.SearchText;
            Filter = searchMetadata.Filter;
            Highlight = searchMetadata.Highlight;
            UseRegex = searchMetadata.UseRegex;
            IgnoreCase = searchMetadata.IgnoreCase;
            RegEx = searchMetadata.RegEx;
            Predicate = searchMetadata.Predicate;
            HighlightHue = searchMetadata.HighlightHue;
            IconKind = searchMetadata.IconKind;
            IsGlobal = searchMetadata.IsGlobal;
            IsExclusion = searchMetadata.IsExclusion;
        }

        public SearchMetadata([NotNull] SearchMetadata searchMetadata, int newPosition, bool isGlobal)
        {
            if (searchMetadata == null) throw new ArgumentNullException(nameof(searchMetadata));

            Position = newPosition;
            IsGlobal = isGlobal;
            SearchText = searchMetadata.SearchText;
            Filter = searchMetadata.Filter;
            Highlight = searchMetadata.Highlight;
            UseRegex = searchMetadata.UseRegex;
            IgnoreCase = searchMetadata.IgnoreCase;
            RegEx = searchMetadata.RegEx;
            Predicate = searchMetadata.Predicate;
            HighlightHue = searchMetadata.HighlightHue;
            IconKind = searchMetadata.IconKind;
            IsExclusion = searchMetadata.IsExclusion;

        }

        public SearchMetadata(int position, [NotNull] string searchText, bool filter, 
            HighlightingMode highlight, 
            bool useRegex, 
            bool ignoreCase,
            Hue highlightHue,
            string iconKind,
            bool isGlobal,
            bool isExclusion)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));

            Position = position;
            SearchText = searchText;
            Filter = filter;
            Highlight = highlight;
            UseRegex = useRegex;
            IgnoreCase = ignoreCase;
            HighlightHue = highlightHue;
            IconKind = iconKind;
            IsGlobal = isGlobal;
            IsExclusion = isExclusion;
            RegEx = this.BuildRegEx();
            Predicate = this.BuildPredicate();
        }

        #region Equality

        public bool Equals(SearchMetadata other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Position == other.Position 
                && string.Equals(SearchText, other.SearchText) 
                && Filter == other.Filter 
                && Highlight == other.Highlight 
                && UseRegex == other.UseRegex 
                && IgnoreCase == other.IgnoreCase 
                && Equals(HighlightHue, other.HighlightHue) 
                && string.Equals(IconKind, other.IconKind) 
                && IsGlobal == other.IsGlobal;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SearchMetadata) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position;
                hashCode = (hashCode*397) ^ (SearchText?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ Filter.GetHashCode();
                hashCode = (hashCode*397) ^ Highlight.GetHashCode();
                hashCode = (hashCode*397) ^ UseRegex.GetHashCode();
                hashCode = (hashCode*397) ^ IgnoreCase.GetHashCode();
                hashCode = (hashCode*397) ^ (HighlightHue?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (IconKind?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ IsGlobal.GetHashCode();
                hashCode = (hashCode * 397) ^ IsExclusion.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SearchMetadata left, SearchMetadata right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SearchMetadata left, SearchMetadata right)
        {
            return !Equals(left, right);
        }

        private sealed class EffectsHighlightEqualityComparer : IEqualityComparer<SearchMetadata>
        {
            public bool Equals(SearchMetadata x, SearchMetadata y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;

                var stringComparison = x.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                return string.Equals(x.SearchText, y.SearchText, stringComparison)
                    && x.Highlight == y.Highlight
                    && x.HighlightHue == y.HighlightHue
                    && x.IconKind == y.IconKind
                    && x.UseRegex == y.UseRegex
                    && x.Position == y.Position
                    && x.IgnoreCase == y.IgnoreCase;
            }

            public int GetHashCode(SearchMetadata obj)
            {
                unchecked
                {
                    var comparer = obj.IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
                    var hashCode = (obj.SearchText != null ? comparer.GetHashCode(obj.SearchText) : 0);
                    hashCode = (hashCode*397) ^ obj.Highlight.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.HighlightHue.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.IconKind.GetHashCode();
                    hashCode = (hashCode*397) ^ obj.UseRegex.GetHashCode();
                    hashCode = (hashCode*397) ^ obj.IgnoreCase.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Position.GetHashCode();
                    return hashCode;
                }
            }
        }

        public static IEqualityComparer<SearchMetadata> EffectsHighlightComparer { get; } = new EffectsHighlightEqualityComparer();

        private sealed class EffectsFilterEqualityComparer : IEqualityComparer<SearchMetadata>
        {
            public bool Equals(SearchMetadata x, SearchMetadata y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;

                var stringComparison = x.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                return string.Equals(x.SearchText, y.SearchText, stringComparison)
                       && x.Filter == y.Filter
                       && x.UseRegex == y.UseRegex
                       && x.IgnoreCase == y.IgnoreCase
                       && x.IsExclusion == y.IsExclusion;
            }

            public int GetHashCode(SearchMetadata obj)
            {
                unchecked
                {
                    var comparer = obj.IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
                    var hashCode = (obj.SearchText != null ? comparer.GetHashCode(obj.SearchText) : 0);
                    hashCode = (hashCode*397) ^ obj.Filter.GetHashCode();
                    hashCode = (hashCode*397) ^ obj.UseRegex.GetHashCode();
                    hashCode = (hashCode*397) ^ obj.IgnoreCase.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.IgnoreCase.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.IsExclusion.GetHashCode();
                    return hashCode;
                }
            }
        }

        public static IEqualityComparer<SearchMetadata> EffectsFilterComparer { get; } = new EffectsFilterEqualityComparer();


        private sealed class SearchTextEqualityComparer : IEqualityComparer<SearchMetadata>
        {
            public bool Equals(SearchMetadata x, SearchMetadata y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.SearchText, y.SearchText);
            }

            public int GetHashCode(SearchMetadata obj)
            {
                return obj.SearchText?.GetHashCode() ?? 0;
            }
        }

        public static IEqualityComparer<SearchMetadata> SearchTextComparer { get; } = new SearchTextEqualityComparer();

        #endregion

        public override string ToString()
        {
            return $"{SearchText} ({Position}) Filter: {Filter}, Highlight: {Highlight}, UseRegex: {UseRegex}, IgnoreCase: {IgnoreCase}";
        }
    }
}