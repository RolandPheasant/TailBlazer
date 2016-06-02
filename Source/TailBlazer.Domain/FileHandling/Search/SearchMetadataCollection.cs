using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Kernel;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling.Search
{
    public class CombinedSearchMetadataCollection : ICombinedSearchMetadataCollection
    {
        private readonly IDisposable _cleanUp;

        public IObservableCache<SearchMetadata, string> Metadata { get; }

        public CombinedSearchMetadataCollection([NotNull] ISearchMetadataCollection metadataCollection,
            [NotNull] IGlobalSearchOptions globalSearchOptions)
        {
            if (metadataCollection == null) throw new ArgumentNullException(nameof(metadataCollection));
            if (globalSearchOptions == null) throw new ArgumentNullException(nameof(globalSearchOptions));

            var cache = new SourceCache<SearchMetadata, string>(t => t.SearchText);

            ////Prioritise local before global and renumber
            var localItems = metadataCollection.Metadata
                .Connect().ToCollection()
                .Select(items => items.ToArray())
                .StartWith(Enumerable.Empty<SearchMetadata>());

            var globalItems = globalSearchOptions.Metadata.Metadata
                .Connect().ToCollection()
                .Select(items => items.ToArray())
                .StartWith(Enumerable.Empty<SearchMetadata>());


            var combiner = localItems.CombineLatest(globalItems, (local, global) =>
            {
                int i = 0;
                var dictionary = new Dictionary<string, SearchMetadata>();

                foreach (var meta in local)
                {
                    dictionary[meta.SearchText] = new SearchMetadata(meta, i);
                    i++;
                }

                foreach (var meta in global)
                {
                    if (dictionary.ContainsKey(meta.SearchText)) continue;
                    dictionary[meta.SearchText] = new SearchMetadata(meta, i);
                    i++;
                }

                return dictionary.Values;
            }).Subscribe(uppdatedItems =>
            {
                cache.Edit(innerCache =>
                {
                    var toRemove = innerCache.Items.Except(uppdatedItems).ToArray();
                    innerCache.Remove(toRemove);
                    innerCache.AddOrUpdate(uppdatedItems);
                });
            });

            Metadata = cache.Connect()
                .IgnoreUpdateWhen((current, previous) => current.Equals(previous))
                .AsObservableCache();

            _cleanUp = new CompositeDisposable(Metadata, cache, combiner);

        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }

    public sealed class SearchMetadataCollection : ISearchMetadataCollection
    {
        private readonly ILogger _logger;
        private readonly ISourceCache<SearchMetadata, string> _searches = new SourceCache<SearchMetadata, string>(t => t.SearchText);
        public IObservableCache<SearchMetadata, string> Metadata { get; }

        private readonly IDisposable _cleanUp;

        public SearchMetadataCollection([NotNull] ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger;
            Metadata = _searches.AsObservableCache();

            _cleanUp = new CompositeDisposable(_searches, Metadata);
        }

        public int NextIndex()
        {
            if (_searches.Count == 0)
                return 0;

            return _searches.Items.Select(m => m.Position).Max() + 1;
        }

        public void Add([NotNull] IEnumerable<SearchMetadata> metadata)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            var searchMetadatas = metadata.AsArray();
            _searches.AddOrUpdate(searchMetadatas);
            _logger.Info("{0} SearchMetadata has been loaded", searchMetadatas.Count());

        }

        public void AddorUpdate([NotNull] SearchMetadata metadata)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            _searches.AddOrUpdate(metadata);
            _logger.Info("Search metadata has changed: {0}", metadata);

        }

        public void Remove(string searchText)
        {
            _searches.Remove(searchText);
            _logger.Info("Search metadata has been removed: {0}", searchText);
        }
        
        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}