using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Dragablz;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Views.Formatting;

namespace TailBlazer.Views.Searching
{
    public class MyVerticalPositionMonitor: MyStackPositionMonitor
    {
        public MyVerticalPositionMonitor() : base(Orientation.Vertical)
        {
        }
    }

    public abstract class MyStackPositionMonitor : PositionMonitor
    {
        private readonly Func<DragablzItem, double> _getLocation;

        protected MyStackPositionMonitor(Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.Horizontal:
                    _getLocation = item => item.X;
                    break;
                case Orientation.Vertical:
                    _getLocation = item => item.Y;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("orientation");
            }
        }

        public event EventHandler<OrderChangedEventArgs> OrderChanged;

        internal virtual void OnOrderChanged(OrderChangedEventArgs e)
        {
            var handler = OrderChanged;
            if (handler != null) handler(this, e);
        }


        internal IEnumerable<DragablzItem> Sort(IEnumerable<DragablzItem> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            return items.OrderBy(i => _getLocation(i));
        }


    }


    public class MyVerticalOrganiser : StackOrganiser
    {
        public MyVerticalOrganiser() : base(Orientation.Vertical)
        {
        }

   

        public override IEnumerable<DragablzItem> Sort(IEnumerable<DragablzItem> items)
        {
            return base.Sort(items);
        }
    }

    public class SearchOptionsViewModel: AbstractNotifyPropertyChanged, IDisposable
    {
        public Guid Id { get; } = Guid.NewGuid();
        private readonly IDisposable _cleanUp;
        private  ReadOnlyObservableCollection<SearchOptionsProxy> _data;

        public ReadOnlyObservableCollection<SearchOptionsProxy> Data
        {
            get { return _data; }
            set { SetAndRaise(ref _data,value);}
  
        }

        public MyVerticalPositionMonitor PositionMonitor { get; } = new MyVerticalPositionMonitor();
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
                //.Throttle(TimeSpan.FromMilliseconds(125))
                .Subscribe(_ => positionMonitor.Disposable = MonitorPositionalChanges()
                                                                .Subscribe(positionChangedArgs =>
                                                                {
                                                                    positionChangedArgs.ForEach(metadataCollection.AddorUpdate);
                                                                }));



            //load data onto grid
            var collection = new ObservableCollectionExtended<SearchOptionsProxy>();
           ReadOnlyObservableCollection<SearchOptionsProxy> data;

            var userOptions = proxyItems.Connect()
                .Sort(SortExpressionComparer<SearchOptionsProxy>.Ascending(proxy => proxy.Position))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(collection,new ObservableCollectionAdaptor<SearchOptionsProxy, string>(0))
                .DisposeMany()
                .Subscribe();

            Data = new ReadOnlyObservableCollection<SearchOptionsProxy>(collection);

            //command to add the current search to the tail collection
            var searchInvoker = SearchHints.SearchRequested.Subscribe(request =>
            {
                schedulerProvider.Background.Schedule(() =>
                {
                  //  Data = new ReadOnlyObservableCollection<SearchOptionsProxy>(new ObservableCollection<SearchOptionsProxy>());

  
                    //var xxx = collection.Clear();
                    var meta = searchMetadataFactory.Create(request.Text, 
                            request.UseRegEx,
                            metadataCollection.NextIndex(), 
                            false);
                    metadataCollection.AddorUpdate(meta);

                    //Observable.Timer(TimeSpan.FromMilliseconds(25))
                    //    .Subscribe(_ =>
                    //    {
                    //        Data = collection;
                    //    });
                   
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