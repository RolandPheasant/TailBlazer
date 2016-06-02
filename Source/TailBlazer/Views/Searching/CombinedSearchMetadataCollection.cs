using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Views.Searching
{
    public class CombinedSearchMetadataCollection: IDisposable
    {
        private readonly IDisposable _cleanUp;

        IObservableCache<SearchMetadata, string> Metadata { get; }

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

            var globalItems = globalSearchOptions.MetadataCollection.Metadata
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
                .IgnoreUpdateWhen((current,previous)=> current.Equals(previous))
                .AsObservableCache();


            _cleanUp =new CompositeDisposable(Metadata, cache, combiner);

        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}