using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Domain.FileHandling.Search
{
    public sealed class SearchInfoCollection : ISearchInfoCollection
    {
        private readonly IDisposable _cleanUp;
        private readonly IColourProvider _colourProvider;
        private readonly IDefaultIconSelector _defaultIconSelector;
        private readonly IEnumerable<IFileWatcher> _filesWatcher;
        private readonly ISearchMetadataCollection _metadataCollection;
        private readonly ISearchMetadataFactory _searchMetadataFactory;

        public SearchInfoCollection(ISearchMetadataCollection searchMetadataCollection,
            ISearchMetadataFactory searchMetadataFactory,
            IColourProvider colourProvider,
            IEnumerable<IFileWatcher> filesWatcher,
            IDefaultIconSelector defaultIconSelector)
        {
            _metadataCollection = searchMetadataCollection;
            _searchMetadataFactory = searchMetadataFactory;
            _colourProvider = colourProvider;
            _filesWatcher = filesWatcher;
            _defaultIconSelector = defaultIconSelector;

            //Add a complete file display
            All = _filesWatcher
                .Select(t => t.Latest)
                .Index()
                .Replay(1)
                .RefCount();

            //create a collection with 1 item, which is used to show entire file
            var systemSearches = new SourceCache<SearchInfo, string>(t => t.SearchText);
            systemSearches.AddOrUpdate(new SearchInfo("<All>", All, SearchType.All));

            //create a collection of all possible user filters
            var userSearches = searchMetadataCollection.Metadata
                .Connect(meta => meta.Filter)
                .IgnoreUpdateWhen((current, previous) => SearchMetadata.EffectsFilterComparer.Equals(current, previous))
                .Transform(meta =>
                {
                    var latest = _filesWatcher
                        .Select(t => t.Latest
                            .Search(meta.BuildPredicate()))
                        .Merge().Replay(1).RefCount();

                    return new SearchInfo(meta.SearchText, latest, SearchType.User);
                });

            //combine te results into a single collection
            Searches = systemSearches.Connect()
                .Or(userSearches)
                .AsObservableCache();

            _cleanUp = new CompositeDisposable(Searches, systemSearches);
        }

        public IObservableCache<SearchInfo, string> Searches { get; }
        public IObservable<ILineProvider> All { get; }

        public void Add([NotNull] string searchText, bool useRegex)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));

            var index = _metadataCollection.NextIndex();
            var metatdata = _searchMetadataFactory.Create(searchText, useRegex, index, true);
            _metadataCollection.AddorUpdate(metatdata);
        }

        public void Remove(string searchText)
        {
            _metadataCollection.Remove(searchText);
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}