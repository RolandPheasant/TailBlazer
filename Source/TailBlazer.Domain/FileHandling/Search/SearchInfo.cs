using System;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling.Search
{
    public sealed class SearchInfo : IEquatable<SearchInfo>
    {
        public string SearchText { get;  }
        public bool IsGlobal { get;  }

        public IObservable<Func<string, bool>> Filter { get; }

        public SearchType SearchType { get;  }
        
        public SearchInfo([NotNull] string searchText, bool isGlobal, [NotNull] IObservable<Func<string, bool>> filter, SearchType searchType)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            SearchText = searchText;
            IsGlobal = isGlobal;
            Filter = filter;
            SearchType = searchType;
        }
        
        #region Equality

        public bool Equals(SearchInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(SearchText, other.SearchText);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SearchInfo)obj);
        }

        public override int GetHashCode()
        {
            return SearchText?.GetHashCode() ?? 0;
        }

        public static bool operator ==(SearchInfo left, SearchInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SearchInfo left, SearchInfo right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"{SearchText}";
        }

    }
}