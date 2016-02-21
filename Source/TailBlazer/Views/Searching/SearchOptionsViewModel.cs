using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Dragablz;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Views.Formatting;

namespace TailBlazer.Views.Searching
{

    public class SearchOptionsViewModel: AbstractNotifyPropertyChanged, IDisposable
    {
        public Guid Id { get; } = Guid.NewGuid();
        private readonly IDisposable _cleanUp;

        public ReadOnlyObservableCollection<SearchOptionsProxy> Data { get; }

        public VerticalPositionMonitor PositionMonitor { get; } = new VerticalPositionMonitor();
        public SearchHints SearchHints { get;  }

        public SearchOptionsViewModel(ISearchMetadataCollection metadataCollection,
            ISearchMetadataFactory searchMetadataFactory,
            ISchedulerProvider schedulerProvider,
            IColourProvider colourProvider,
            IIconProvider iconsProvider,
            SearchHints searchHints)
        {

            SearchHints = searchHints;

            var proxyItems = metadataCollection.Metadata.Connect()
                .WhereReasonsAre(ChangeReason.Add, ChangeReason.Remove) //ignore updates because we update from here
                .Transform(meta =>
                {
                    return new SearchOptionsProxy(meta,
                        colourProvider,
                        new IconSelector(iconsProvider, schedulerProvider),
                        m => metadataCollection.Remove(m.SearchText),
                        iconsProvider.DefaultIconSelector,
                        Id);
                })
                .SubscribeMany(so =>
                {
                    //when a value changes, write the original value back to the cache
                    return so.WhenAnyPropertyChanged()
                        .Select(_ => (SearchMetadata)so)
                        .Subscribe(metadataCollection.AddorUpdate);
                })
                .AsObservableCache();

            var monitor = MonitorPositionalChanges()
                            .Subscribe(positionChangedArgs =>
                            {
                                positionChangedArgs.ForEach(metadataCollection.AddorUpdate);
                            });

            
            //load data onto grid
            var collection = new ObservableCollectionExtended<SearchOptionsProxy>();

            var userOptions = proxyItems.Connect()
                .Sort(SortExpressionComparer<SearchOptionsProxy>.Ascending(proxy => proxy.Position))
                .ObserveOn(schedulerProvider.MainThread)
                //force reset for each new or removed item dues to a bug in the underlying dragablz control which inserts in an incorrect position
                .Bind(collection,new ObservableCollectionAdaptor<SearchOptionsProxy, string>(0))
                .DisposeMany()
                .Subscribe();

            Data = new ReadOnlyObservableCollection<SearchOptionsProxy>(collection);

            //command to add the current search to the tail collection
            var searchInvoker = SearchHints.SearchRequested.Subscribe(request =>
            {
                schedulerProvider.Background.Schedule(() =>
                {
                    var meta = searchMetadataFactory.Create(request.Text, 
                            request.UseRegEx,
                            metadataCollection.NextIndex(), 
                            false);
                    metadataCollection.AddorUpdate(meta);
                   
                });
            });
            
            _cleanUp = new CompositeDisposable(searchInvoker,
                userOptions,
                searchInvoker,
                monitor,
                SearchHints);
        }

        private IObservable<IEnumerable<SearchMetadata>> MonitorPositionalChanges()
        {
            return Observable.FromEventPattern<OrderChangedEventArgs>(
                h => PositionMonitor.OrderChanged += h,
                h => PositionMonitor.OrderChanged -= h)
                .Throttle(TimeSpan.FromMilliseconds(125))
                .Select(evt => evt.EventArgs)
                .Where(args => args.PreviousOrder != null && !args.PreviousOrder.SequenceEqual(args.NewOrder))
                .Select(positionChangedArgs =>
                {
                    var newOrder = positionChangedArgs.NewOrder
                        .OfType<SearchOptionsProxy>()
                        .Select((item, index) =>
                        {
                            item.Position = index;
                            return new {Meta = (SearchMetadata) item, index};
                        })
                        .ToArray();


                    //reprioritise filters and highlights
                    return newOrder
                        .Select(x => new SearchMetadata(x.Meta, x.index))
                        .ToArray();
                });
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}