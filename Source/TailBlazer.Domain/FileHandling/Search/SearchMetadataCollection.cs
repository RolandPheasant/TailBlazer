using System;
using System.Reactive.Disposables;
using DynamicData;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling.Search
{
    public sealed class SearchMetadataCollection : ISearchMetadataCollection
    {
        private readonly ISourceCache<SearchMetadata, CaseInsensitiveString> _searches = new SourceCache<SearchMetadata, CaseInsensitiveString>(t => (CaseInsensitiveString)t.SearchText);
        public IObservableCache<SearchMetadata, CaseInsensitiveString> Metadata { get; }

        private readonly IDisposable _cleanUp;

        public SearchMetadataCollection()
        {
            Metadata = _searches.AsObservableCache();
            _cleanUp = new CompositeDisposable(_searches, Metadata);
        }

        public void Add([NotNull] SearchMetadata metadata)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            _searches.AddOrUpdate(metadata);
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