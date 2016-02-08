using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Dragablz;
using DynamicData;
using DynamicData.Binding;
using MaterialDesignColors;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Infrastructure;


namespace TailBlazer.Views.Searching
{
    public class SearchOptionsViewModel: AbstractNotifyPropertyChanged, IDisposable
    {
        //create text to add new option - default to highlight without search
        private readonly IDisposable _cleanUp;
        private string _searchText;

        public ReadOnlyObservableCollection<SearchOptionsProxy> Data { get; }

        // public VerticalOrganiser Organiser { get; } = new VerticalOrganiser();

        public VerticalPositionMonitor PositionMonitor { get; } = new VerticalPositionMonitor();

        public SearchHints SearchHints { get;  }

        public SearchOptionsViewModel(ISearchMetadataCollection metadataCollection, 
            ISchedulerProvider schedulerProvider,
            SearchHints searchHints)
        {
            SearchHints = searchHints;
            //TODO: options for colour

            var swatches = new SwatchesProvider().Swatches;

            bool binding = false;

            var orderChanged = Observable.FromEventPattern<OrderChangedEventArgs>(
                                            h => PositionMonitor.OrderChanged += h,
                                            h => PositionMonitor.OrderChanged -= h)
                                    .Throttle(TimeSpan.FromMilliseconds(125))
                                    .Select(evt => evt.EventArgs)
                                    .Where(args=>args.PreviousOrder!=null && args.NewOrder.Length == args.PreviousOrder.Length)
                                    .Select(positionChangedArgs =>
                                    {
                                            //reprioritise filters and highlights
                                            return positionChangedArgs.NewOrder
                                            .OfType<SearchOptionsProxy>()
                                            .Select((item, index) => new {Meta=(SearchMetadata)item, index})
                                            //.Where(x => x.index != x.Meta.Position)
                                            .Select(x => new SearchMetadata(x.Meta, x.index))
                                            .ToArray();
                                    })
                                    .Subscribe(positionChangedArgs =>
                                    {
                                        positionChangedArgs.ForEach(metadataCollection.AddorUpdate);
                                    });

            ReadOnlyObservableCollection<SearchOptionsProxy> data;

            var userOptions = metadataCollection.Metadata.Connect()
                .WhereReasonsAre(ChangeReason.Add, ChangeReason.Remove) //ignore updates because we update from here
                .Transform(meta => new SearchOptionsProxy(meta, swatches, m => metadataCollection.Remove(m.SearchText)))
                .SubscribeMany(so =>
                {
                    //when a value changes, write the original value back to the cache
                    return so.WhenAnyPropertyChanged()
                    .Select(_=> new SearchMetadata(so.Position,so.Text, so.Filter, so.Highlight, so.UseRegex, so.IgnoreCase))
                    .Subscribe(metadataCollection.AddorUpdate);
                })
                .Sort(SortExpressionComparer<SearchOptionsProxy>.Ascending(proxy => proxy.Position))

                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out data)
                .Subscribe();
            
            Data = data;

            //command to add the current search to the tail collection
            var searchInvoker = SearchHints.SearchRequested.Subscribe(request =>
            {
                schedulerProvider.Background.Schedule(() =>
                {
                    metadataCollection.AddorUpdate(new SearchMetadata(metadataCollection.NextIndex(), request.Text, false, true, request.UseRegEx, true));
                });
            });
        
            
            _cleanUp = new CompositeDisposable(searchInvoker, 
                userOptions, 
                searchInvoker,
                orderChanged);
        }

        public string SearchText
        {
            get { return _searchText; }
            set { SetAndRaise(ref _searchText, value); }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}