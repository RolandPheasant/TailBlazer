using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Views.Searching
{
    public class GlobalSearchOptions : IGlobalSearchOptions, IDisposable
    {
        private readonly IDisposable _cleanUp;

        public ISearchMetadataCollection Metadata { get; }

        public GlobalSearchOptions(ISearchMetadataCollection metadata,
            ISearchStateToMetadataMapper converter,
            ISetting<SearchState[]> searchStateSettings)
        {
            Metadata = metadata;

            var loader = searchStateSettings.Value
                .Take(1)
                .Select(items => items.Select(state => converter.Map(state,true)))
                .Subscribe(metadata.Add);

            var writer = metadata.Metadata.Connect()
                .ToCollection()
                .Select(metaData => metaData.ToArray())
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Select(searchStateItems => searchStateItems.Select(converter.Map).ToArray())
                .Subscribe(searchStateSettings.Write);

            _cleanUp = new CompositeDisposable(loader, writer);
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }

    }
}