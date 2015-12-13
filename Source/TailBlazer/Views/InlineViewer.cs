using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using DynamicData;
using TailBlazer.Domain.Annotations;
using DynamicData.Binding;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views
{
    public class InlineViewer : AbstractNotifyPropertyChanged, IScrollReceiver,IDisposable
    {
        private readonly IDisposable _cleanUp;
        private readonly ReadOnlyObservableCollection<LineProxy> _data;
        private readonly ISubject<ScrollRequest> _userScrollRequested = new ReplaySubject<ScrollRequest>(1);
        private int _firstIndex;
        private int _pageSize;

        public ReadOnlyObservableCollection<LineProxy> Lines => _data;
        public IProperty<int> Count { get; }
        public ICommand CopyToClipboardCommand { get; }

        public ISelectionMonitor SelectionMonitor { get; }
        public InlineViewer([NotNull] InlineViewerArgs args,
            [NotNull] IClipboardHandler clipboardHandler,
            [NotNull] ISchedulerProvider schedulerProvider, 
            [NotNull] ISelectionMonitor selectionMonitor)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (clipboardHandler == null) throw new ArgumentNullException(nameof(clipboardHandler));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            if (selectionMonitor == null) throw new ArgumentNullException(nameof(selectionMonitor));
            SelectionMonitor = selectionMonitor;
            CopyToClipboardCommand = new Command(() => clipboardHandler.WriteToClipboard(selectionMonitor.GetSelectedText()));

            var lineProvider = args.LineProvider;
            var selectedChanged = args.SelectedChanged;
            var scrollSelected = selectedChanged.Where(proxy => proxy != null)
                    .CombineLatest(lineProvider, (proxy, lp) => new ScrollRequest(10, (int) proxy.Number, true));

            var scrollUser = _userScrollRequested
                .Select(request => new ScrollRequest(ScrollReason.User, request.PageSize, request.FirstIndex));
            
            var scroller= scrollSelected.Merge(scrollUser)
                .ObserveOn(schedulerProvider.Background)
                .DistinctUntilChanged();
            
            var lineScroller = new LineScroller(lineProvider, scroller);
            Count = lineProvider.Select(lp=>lp.Count).ForBinding();
            
            //load lines into observable collection
            var loader = lineScroller.Lines.Connect()
                .Transform(line => new LineProxy(line))
                .Sort(SortExpressionComparer<LineProxy>.Ascending(proxy => proxy))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _data)
                .Subscribe();
            
            //track first visible index [required to set scroll extent]
            var firstIndexMonitor = lineScroller.Lines.Connect()
                .Buffer(TimeSpan.FromMilliseconds(250)).FlattenBufferResult()
                .QueryWhenChanged(lines => lines.Count == 0 ? 0 : lines.Select(l => l.Index).Min())
                .Subscribe(first => FirstIndex = first);

            _cleanUp = new CompositeDisposable(lineScroller,
                        loader,
                        Count,
                        firstIndexMonitor,
                        Disposable.Create(() =>
                        {
                            _userScrollRequested.OnCompleted();
                        }));
        }


        void IScrollReceiver.ScrollBoundsChanged(ScrollBoundsArgs boundsArgs)
        {
            if (boundsArgs == null) throw new ArgumentNullException(nameof(boundsArgs));
            _userScrollRequested.OnNext(new ScrollRequest(ScrollReason.User, boundsArgs.PageSize, boundsArgs.FirstIndex));
            PageSize = boundsArgs.PageSize;
            FirstIndex = boundsArgs.FirstIndex;
        }

        void IScrollReceiver.ScrollChanged(ScrollChangedArgs scrollChangedArgs)
        {
        }

        public int PageSize
        {
            get { return _pageSize; }
            set { SetAndRaise(ref _pageSize, value); }
        }

        public int FirstIndex
        {
            get { return _firstIndex; }
            set { SetAndRaise(ref _firstIndex, value); }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}