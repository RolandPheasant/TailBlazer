using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling.Search
{
    public class CombinedSearchMetadataCollection : ICombinedSearchMetadataCollection
    {
        private readonly IDisposable _cleanUp;

        public IObservableCache<SearchMetadata, string> Combined { get; }

        public ISearchMetadataCollection Local { get; }

        public ISearchMetadataCollection Global { get; }

        public CombinedSearchMetadataCollection([NotNull] ISearchMetadataCollection metadataCollection, [NotNull] IGlobalSearchOptions globalSearchOptions)
        {
            if (metadataCollection == null) throw new ArgumentNullException(nameof(metadataCollection));
            if (globalSearchOptions == null) throw new ArgumentNullException(nameof(globalSearchOptions));

            Local = metadataCollection;
            Global = globalSearchOptions.Metadata;

            var cache = new SourceCache<SearchMetadata, string>(t => t.SearchText);

            ////Prioritise local before global and renumber
            var localItems = metadataCollection.Metadata
                .Connect().ToCollection()
                .Select(items => items.ToArray<SearchMetadata>())
                .StartWith(Enumerable.Empty<SearchMetadata>());

            var globalItems = globalSearchOptions.Metadata.Metadata
                .Connect().ToCollection()
                .Select(items => items.ToArray())
                .StartWith(Enumerable.Empty<SearchMetadata>());

            var combiner = localItems.CombineLatest(globalItems, (local, global) => new {local, global})
                .Select(x => Combine(x.local, x.global))
                .Subscribe(uppdatedItems =>
                {
                    cache.Edit(innerCache =>
                    {
                        var toRemove = innerCache.Items.Except(uppdatedItems).ToArray();
                        innerCache.Remove(toRemove);
                        innerCache.AddOrUpdate(uppdatedItems);
                    });
                });

            Combined = cache.Connect()
                .IgnoreUpdateWhen((current, previous) => current.Equals(previous))
                .AsObservableCache();

            _cleanUp = new CompositeDisposable(Combined, cache, combiner);
        }

        private SearchMetadata[] Combine(IEnumerable<SearchMetadata> local, IEnumerable<SearchMetadata> global)
        {
            int i = 0;
            var dictionary = new Dictionary<string, SearchMetadata>();

            foreach (var meta in local.OrderBy(meta=>meta.Position))
            {
                dictionary[meta.SearchText] = new SearchMetadata(meta, i);
                i++;
            }

            foreach (var meta in global.OrderBy(meta => meta.Position))
            {
                if (dictionary.ContainsKey(meta.SearchText)) continue;
                dictionary[meta.SearchText] = new SearchMetadata(meta, i);
                i++;
            }

            return dictionary.Values.ToArray();
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}