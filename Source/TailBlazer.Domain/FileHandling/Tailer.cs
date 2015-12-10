using System;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public sealed class Tailer : IEquatable<Tailer>
    {
        public string SearchText { get;  }
        public IObservable<ILineProvider> Latest { get;  }
        public bool IsDefault { get;  }

        public Tailer([NotNull] string searchText, [NotNull] IObservable<ILineProvider> latest, bool isDefault)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));
            if (latest == null) throw new ArgumentNullException(nameof(latest));
            SearchText = searchText;
            Latest = latest;
            IsDefault = isDefault;
        }
        
        #region Equality

        public bool Equals(Tailer other)
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
            return Equals((Tailer)obj);
        }

        public override int GetHashCode()
        {
            return SearchText?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Tailer left, Tailer right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Tailer left, Tailer right)
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