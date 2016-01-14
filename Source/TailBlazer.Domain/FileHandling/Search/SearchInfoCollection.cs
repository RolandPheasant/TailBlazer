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
        private readonly IDisposable _cleanUp;

        public IObservableCache<SearchInfo, CaseInsensitiveString> Searches { get; }
        
        public IObservable<ILineProvider> All { get; }
        
        public SearchInfoCollection(ISearchMetadataCollection searchMetadataCollection, IFileWatcher fileWatcher)
        {
            _metadataCollection = searchMetadataCollection;
            _fileWatcher = fileWatcher;

            //Add a complete file display
            All = fileWatcher.Latest.Index().Replay(1).RefCount();

            //create a collection with 1 item, which is used to show entire file
            var systemSearches = new SourceCache<SearchInfo, CaseInsensitiveString>(t => (CaseInsensitiveString)t.SearchText);
            systemSearches.AddOrUpdate(new SearchInfo("<All>", All, SearchType.All));


            //create a collection of all possible user filters
            var userSearches = searchMetadataCollection.Metadata
                .Connect(meta => meta.Filter)
                .IgnoreUpdateWhen((current,previous)=> SearchMetadata.EffectsFilterComparer.Equals(current, previous))
                .Transform(meta =>
                {
                    var latest = _fileWatcher.Latest
                        .Search(meta.BuildPredicate())
                        .Replay(1).RefCount();

                    return new SearchInfo(meta.SearchText, latest, SearchType.User);
                });

            //combine te results into a single collection
            Searches = systemSearches.Connect()
                    .Or(userSearches)
                    .AsObservableCache();

            _cleanUp = new CompositeDisposable(Searches, systemSearches);
        }

        public void Add([NotNull] string searchText, bool useRegex)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));
            _metadataCollection.Add(new SearchMetadata(searchText,true,true, useRegex,true));
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
