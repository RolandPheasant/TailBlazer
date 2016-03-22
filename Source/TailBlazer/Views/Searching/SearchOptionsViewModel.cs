using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Dragablz;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.FileHandling.TextAssociations;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Views.Formatting;

namespace TailBlazer.Views.Searching
{
    public class SearchOptionsViewModel : AbstractNotifyPropertyChanged, IDisposable
    {
        public Guid Id { get; } = Guid.NewGuid();
        private readonly IDisposable _cleanUp;

        public ReadOnlyObservableCollection<SearchOptionsProxy> Data { get; }
        public VerticalPositionMonitor PositionMonitor { get; } = new VerticalPositionMonitor();
        public SearchHints SearchHints { get; }

        public SearchOptionsViewModel(ISearchMetadataCollection metadataCollection,
            ISearchMetadataFactory searchMetadataFactory,
            ISchedulerProvider schedulerProvider,
            IColourProvider colourProvider,
            IIconProvider iconsProvider,
            ITextAssociationCollection textAssociationCollection,
            SearchHints searchHints,
            IThemeProvider themeProvider)
        {
            SearchHints = searchHints;

            var proxyItems = metadataCollection.Metadata.Connect()
                .WhereReasonsAre(ChangeReason.Add, ChangeReason.Remove) //ignore updates because we update from here
                .Transform(meta =>
                {
                    return new SearchOptionsProxy(meta,
                        colourProvider,
                        themeProvider,
                        new IconSelector(iconsProvider, schedulerProvider),
                        m => metadataCollection.Remove(m.SearchText),
                        iconsProvider.DefaultIconSelector,
                        Id);
                })
                .SubscribeMany(so =>
                {
                    //when a value changes, write the original value back to the metadata collection
                    var anyPropertyHasChanged = so.WhenAnyPropertyChanged()
                        .Select(_ => (SearchMetadata) so)
                        .Subscribe(metadataCollection.AddorUpdate);
                    
                    //when an icon or colour has changed we need to record user choice so 
                    //the same choice can be used again
                    var iconChanged = so.WhenValueChanged(proxy => proxy.IconKind,false).ToUnit(); 
                    var colourChanged = so.WhenValueChanged(proxy => proxy.HighlightHue, false).ToUnit();
                    var ignoreCaseChanged = so.WhenValueChanged(proxy => proxy.IgnoreCase, false).ToUnit();
                    
                    var textAssociationChanged = iconChanged.Merge(colourChanged).Merge(ignoreCaseChanged)
                                .Throttle(TimeSpan.FromMilliseconds(250))
                                .Select(_=> new TextAssociation(so.Text, so.IgnoreCase, so.UseRegex, so.HighlightHue.Swatch, so.IconKind.ToString(), so.HighlightHue.Name, DateTime.Now))
                                .Subscribe(textAssociationCollection.MarkAsChanged);
                    
                    return new CompositeDisposable(anyPropertyHasChanged, textAssociationChanged);
                })
                .AsObservableCache();

            var monitor = MonitorPositionalChanges()
                .Subscribe(metadataCollection.Add);


            //load data onto grid
            var collection = new ObservableCollectionExtended<SearchOptionsProxy>();

            var userOptions = proxyItems.Connect()
                .Sort(SortExpressionComparer<SearchOptionsProxy>.Ascending(proxy => proxy.Position))
                .ObserveOn(schedulerProvider.MainThread)
                //force reset for each new or removed item dues to a bug in the underlying dragablz control which inserts in an incorrect position
                .Bind(collection, new ObservableCollectionAdaptor<SearchOptionsProxy, string>(0))
                .DisposeMany()
                .Subscribe();

            Data = new ReadOnlyObservableCollection<SearchOptionsProxy>(collection);

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