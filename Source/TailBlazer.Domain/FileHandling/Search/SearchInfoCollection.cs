using System;
using System.Reactive.Disposables;
using DynamicData;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling.Search
{
    public sealed class SearchInfoCollection : ISearchInfoCollection
    {
        private readonly ISourceCache<SearchInfo, CaseInsensitiveString> _searches = new SourceCache<SearchInfo, CaseInsensitiveString>(t => (CaseInsensitiveString)t.SearchText);

        public IObservableCache<SearchInfo, CaseInsensitiveString> Searches { get; }

        private readonly IDisposable _cleanUp;

        public SearchInfoCollection(ISearchMetadataCollection metadataCollection)
        {
            //var filters = metadataCollection.Metadata
            //                    .Connect(m=>m.Filter)
            //                    .Transform(meta=>new SearchInfo(meta.SearchText,))

            Searches = _searches.AsObservableCache();
            _cleanUp = new CompositeDisposable(_searches, Searches);
        }

        public void Add([NotNull] string searchText, [NotNull] IObservable<ILineProvider> latest,  SearchType type = SearchType.User)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));
            if (latest == null) throw new ArgumentNullException(nameof(latest));

            if (type == SearchType.All)
            {
                
            }

            _searches.AddOrUpdate(new SearchInfo(searchText, latest, type));
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
