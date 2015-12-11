using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;

using DynamicData;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public sealed class SearchInfoCollection : ISearchInfoCollection
    {
        private readonly ISourceCache<SearchInfo, CaseInsensitiveString> _searches = new SourceCache<SearchInfo, CaseInsensitiveString>(t => (CaseInsensitiveString)t.SearchText);
        public IObservableCache<SearchInfo, CaseInsensitiveString> Searches { get; }

        private readonly IDisposable _cleanUp;

        public SearchInfoCollection()
        {
            Searches = _searches.AsObservableCache();
            _cleanUp = new CompositeDisposable(_searches, Searches);
        }

        public void Add([NotNull] string searchText, [NotNull] IObservable<ILineProvider> latest, bool isDefault = false)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));
            if (latest == null) throw new ArgumentNullException(nameof(latest));
            _searches.AddOrUpdate(new SearchInfo(searchText, latest, isDefault));
        }

        public void Remove(string searchText)
        {
            _searches.Remove((CaseInsensitiveString)searchText);
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
