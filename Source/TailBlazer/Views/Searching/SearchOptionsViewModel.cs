using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData.Binding;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Views.Searching
{
    public class SearchOptionsViewModel : AbstractNotifyPropertyChanged, IDisposable
    {
        public Guid Id { get; } = Guid.NewGuid();

        private readonly IDisposable _cleanUp;
        public SearchHints SearchHints { get; }
        public ISearchProxyCollection Local { get; }
        public ISearchProxyCollection Global { get; }

        public SearchOptionsViewModel(ISearchMetadataCollection metadataCollection,
            IGlobalSearchOptions globalSearchOptions,
            ISearchProxyCollectionFactory searchProxyCollectionFactory,
            ISearchMetadataFactory searchMetadataFactory,
            ISchedulerProvider schedulerProvider,
            
            SearchHints searchHints)
        {
            SearchHints = searchHints;
            
            Local = searchProxyCollectionFactory.Create(metadataCollection, Id);
            Global = searchProxyCollectionFactory.Create(globalSearchOptions.MetadataCollection, Id);

            //command to add the current search to the tail collection
            var searchInvoker = SearchHints.SearchRequested
                .ObserveOn(schedulerProvider.Background)
                .Subscribe(request =>
                {
                    var meta = searchMetadataFactory.Create(request.Text,
                        request.UseRegEx,
                        metadataCollection.NextIndex(),
                        false);
                    metadataCollection.AddorUpdate(meta);
                });

            _cleanUp = new CompositeDisposable(searchInvoker,
                searchInvoker,
                SearchHints,
                Global,
                Local);
        }
        
        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}