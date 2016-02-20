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
            IAccentColourProvider accentColourProvider,
            IIconProvider iconsProvider,
            SearchHints searchHints)
        {

            SearchHints = searchHints;
            var positionMonitor = new SerialDisposable();

            var proxyItems = metadataCollection.Metadata.Connect()
                .WhereReasonsAre(ChangeReason.Add, ChangeReason.Remove) //ignore updates because we update from here
                .Do(_ => positionMonitor.Disposable=Disposable.Empty)
                .Transform(meta =>
                {
                    return new SearchOptionsProxy(meta,
                        accentColourProvider,
                        new IconSelector(iconsProvider, schedulerProvider),
                        m => metadataCollection.Remove(m.SearchText),
                        iconsProvider.KnownIconNames,
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

            var monitor = proxyItems
                .Connect()
                .Throttle(TimeSpan.FromMilliseconds(125))
                .Subscribe(_ => positionMonitor.Disposable = MonitorPositionalChanges()
                                                                .Subscribe(positionChangedArgs =>
                                                                {
                                                                    positionChangedArgs.ForEach(metadataCollection.AddorUpdate);
                                                                }));

            //load data onto grid
            ReadOnlyObservableCollection<SearchOptionsProxy> data;

            var userOptions = proxyItems.Connect()
                .Sort(SortExpressionComparer<SearchOptionsProxy>.Ascending(proxy => proxy.Position))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out data)
                .DisposeMany()
                .Subscribe();

            Data = data;

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
                positionMonitor,
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
                //.Throttle(TimeSpan.FromMilliseconds(125))
                .Select(evt => evt.EventArgs)
                .Where(args => args.PreviousOrder != null && args.NewOrder.Length == args.PreviousOrder.Length)
                .Select(positionChangedArgs =>
                {
                    //reprioritise filters and highlights
                    return positionChangedArgs.NewOrder
                        .OfType<SearchOptionsProxy>()
                        .Select((item, index) => new {Meta = (SearchMetadata) item, index})
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