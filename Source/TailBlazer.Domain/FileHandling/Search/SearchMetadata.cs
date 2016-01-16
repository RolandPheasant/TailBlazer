using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DynamicData.Kernel;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling.Search
{
    public class SearchMetadata
    {
        public string SearchText { get; }
        public bool Filter { get; }
        public bool Highlight { get; }
        public bool UseRegex { get; }

        public bool IgnoreCase { get; }

        public Optional<Regex> RegEx { get; }

        public Func<string, bool> Predicate { get; }

        public SearchMetadata([NotNull] string searchText, bool filter, bool highlight, bool useRegex, bool ignoreCase)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));

            SearchText = searchText;
            Filter = filter;
            Highlight = highlight;
            UseRegex = useRegex;
            IgnoreCase = ignoreCase;

            RegEx = this.BuildRegEx();
            Predicate = this.BuildPredicate();
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
                    && x.UseRegex == y.UseRegex && x.IgnoreCase == y.IgnoreCase;
            }

            public int GetHashCode(SearchMetadata obj)
            {
                unchecked
                {
                    var comparer = obj.IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
                    var hashCode = (obj.SearchText != null ? comparer.GetHashCode(obj.SearchText) : 0);
                    hashCode = (hashCode*397) ^ obj.Highlight.GetHashCode();
                    hashCode = (hashCode*397) ^ obj.UseRegex.GetHashCode();
                    hashCode = (hashCode*397) ^ obj.IgnoreCase.GetHashCode();
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
                    && x.Filter == y.Filter && x.UseRegex == y.UseRegex && x.IgnoreCase == y.IgnoreCase;
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
                    return hashCode;
                }
            }
        }

        public static IEqualityComparer<SearchMetadata> EffectsFilterComparer { get; } = new EffectsFilterEqualityComparer();
    }
}