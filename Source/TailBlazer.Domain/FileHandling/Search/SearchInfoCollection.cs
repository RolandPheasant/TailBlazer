using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling.Search
{
    public sealed class SearchInfoCollection : ISearchInfoCollection
    {
        private readonly ISearchMetadataCollection _metadataCollection;
        private readonly IFileWatcher _fileWatcher;

        public IObservableCache<SearchInfo, CaseInsensitiveString> Searches { get; }

        public IObservable<ILineProvider> All { get; }

        private readonly IDisposable _cleanUp;

        public SearchInfoCollection(ISearchMetadataCollection metadataCollection, IFileWatcher fileWatcher)
        {
            _metadataCollection = metadataCollection;
            _fileWatcher = fileWatcher;

            //Add a complete file display
            All = fileWatcher.Latest.Index().Replay(1).RefCount();
            
            var systemSearches = new SourceCache<SearchInfo, CaseInsensitiveString>(t => (CaseInsensitiveString)t.SearchText);
            systemSearches.AddOrUpdate(new SearchInfo("<All>", All, SearchType.All));

            var userSearches = metadataCollection.Metadata
                .Connect(meta => meta.Filter)
                .Transform(meta =>
                {
                    var latest = _fileWatcher.Latest
                        .Search(s => s.Contains(meta.SearchText, StringComparison.OrdinalIgnoreCase))
                        .Replay(1).RefCount();

                    return new SearchInfo(meta.SearchText, latest, SearchType.User);
                });

            Searches = systemSearches.Connect()
                    .Or(userSearches)
                    .AsObservableCache();

            _cleanUp = new CompositeDisposable(Searches, systemSearches);
        }

        public void Add([NotNull] string searchText)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));
            _metadataCollection.Add(new SearchMetadata(searchText,true,true));
        }

        public void Remove(string searchText)
        {
            _metadataCollection.Remove((CaseInsensitiveString)searchText);
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
