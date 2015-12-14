using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views
{
    public class FileTailerViewModel: AbstractNotifyPropertyChanged, IDisposable, IScrollReceiver
    {
        private readonly IDisposable _cleanUp;
        private readonly ReadOnlyObservableCollection<LineProxy> _data;
        private readonly ISubject<ScrollRequest> _userScrollRequested = new ReplaySubject<ScrollRequest>(1);

        private string _searchText;
        private bool _autoTail=true;
        private int _firstIndex;
        private int _pageSize;
        private LineProxy _selectedLine;
        private bool _showInline;

        public ReadOnlyObservableCollection<LineProxy> Lines => _data;
        public IProperty<int> SelectedItemsCount { get; }
        public IProperty<bool> ShouldHightlightMatchingText { get; }
        public IProperty<string> SearchHint { get; }
        public IProperty<int> Count { get; }
        public IProperty<string> CountText { get; }
        public IProperty<int> LatestCount { get; }
        public IProperty<string> FileSizeText { get; }
        public IProperty<bool> IsLoading { get; }
        public IProperty<string> HightlightText { get; }
        public ICommand CopyToClipboardCommand { get; }
        public ICommand KeepSearchCommand { get; }

        public ISelectionMonitor SelectionMonitor { get; }
        public SearchCollection SearchCollection { get; }

        public InlineViewer InlineViewer { get; }
        public IProperty<bool> InlineViewerVisible { get; }
        public IProperty<bool> CanViewInline { get; }

        public FileTailerViewModel([NotNull] ILogger logger,
            [NotNull] ISchedulerProvider schedulerProvider,
            [NotNull] IFileWatcher fileWatcher,
            [NotNull] ISelectionMonitor selectionMonitor, 
            [NotNull] IClipboardHandler clipboardHandler, 
            [NotNull] ISearchInfoCollection searchInfoCollection, 
            [NotNull] IInlineViewerFactory inlineViewerFactory)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            if (fileWatcher == null) throw new ArgumentNullException(nameof(fileWatcher));
            if (selectionMonitor == null) throw new ArgumentNullException(nameof(selectionMonitor));
            if (clipboardHandler == null) throw new ArgumentNullException(nameof(clipboardHandler));
            if (searchInfoCollection == null) throw new ArgumentNullException(nameof(searchInfoCollection));
            if (inlineViewerFactory == null) throw new ArgumentNullException(nameof(inlineViewerFactory));

            SelectionMonitor = selectionMonitor;
            CopyToClipboardCommand = new Command(()=> clipboardHandler.WriteToClipboard(selectionMonitor.GetSelectedText()));
            SelectedItemsCount = selectionMonitor.Selected.Connect().QueryWhenChanged(collection=> collection.Count).ForBinding();
            SearchCollection = new SearchCollection(searchInfoCollection, schedulerProvider);
            
            //An observable which acts as a scroll command
            var autoChanged = this.WhenValueChanged(vm => vm.AutoTail);
            var scroller = _userScrollRequested.CombineLatest(autoChanged, (user, auto) =>
                        {
                            var mode = AutoTail ? ScrollReason.Tail : ScrollReason.User;
                            return  new ScrollRequest(mode, user.PageSize, user.FirstIndex);
                        })
                        .ObserveOn(schedulerProvider.Background)
                        .DistinctUntilChanged();

            //tailer is the main object used to tail, scroll and filter in a file
            var lineScroller = new LineScroller(SearchCollection.Latest.ObserveOn(schedulerProvider.Background), scroller);  //fileTailerFactory.Create(fileInfo, SearchCollection.Latest.ObserveOn(schedulerProvider.Background), scroller);

            //Add a complete file display [No search info here]
            var indexed = fileWatcher.Latest.Index()
                .Replay(1).RefCount();

            IsLoading = indexed.Take(1).Select(_ => false).StartWith(true).ForBinding();
            searchInfoCollection.Add("<All>", indexed, SearchType.All);
            
            //command to add the current search to the tail collection
            KeepSearchCommand = new Command(() =>
            {
                var text = SearchText;
                var latest =   fileWatcher.Latest
                    .Search(s => s.Contains(text, StringComparison.OrdinalIgnoreCase))
                    .Replay(1).RefCount();

                searchInfoCollection.Add(text, latest);
                SearchText = string.Empty;
            },()=> SearchText.IsLongerThanOrEqualTo(3));

            //User feedback to show file size
            FileSizeText = fileWatcher.Latest.Select(fn=>fn.Size)
                .Select(size => size.FormatWithAbbreviation())
                .DistinctUntilChanged()
                .ForBinding();

            //User feedback to guide them whilst typing
            SearchHint = this.WhenValueChanged(vm => vm.SearchText)
                            .Select(text =>
                            {
                                if (string.IsNullOrEmpty(text)) return "Type to search";
                                return text.Length < 3 ? "Enter at least 3 characters" : "Hit enter to search";
                            }).ForBinding();

            var selectedText = SearchCollection.SelectedText;

            //Only highlight search text when at least 3 letters have been entered
            HightlightText = selectedText.ForBinding();
            ShouldHightlightMatchingText = selectedText
                .Select(searchText => !string.IsNullOrEmpty(searchText) && searchText.Length >= 3)
                .ForBinding();

            //load lines into observable collection
            var loader = lineScroller.Lines.Connect()
                .Transform(line => new LineProxy(line))
                .Sort(SortExpressionComparer<LineProxy>.Ascending(proxy => proxy))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _data)
                .Subscribe(changes => logger.Info($"Rows changed. {changes.Adds} adds, {changes.Removes} removed"), 
                            ex => logger.Error(ex, "There is a problem with bind data"));
            
            //monitor matching lines and start index,
            Count = indexed.Select(latest=>latest.Count).ForBinding();
            CountText = indexed.Select(latest => $"{latest.Count.ToString("##,###")} lines").ForBinding();
            LatestCount = SearchCollection.Latest.Select(latest => latest.Count).ForBinding();

            //track first visible index
            var firstIndexMonitor = lineScroller.Lines.Connect()
                .Buffer(TimeSpan.FromMilliseconds(250)).FlattenBufferResult()
                .QueryWhenChanged(lines =>lines.Count == 0 ? 0 : lines.Select(l => l.Index).Min())
                .ObserveOn(schedulerProvider.MainThread)
                .Subscribe(first => FirstIndex = first);

            //Create objects required for inline viewing
            var isUserDefinedChanged = SearchCollection.WhenValueChanged(sc => sc.Selected)
                .Select(selected => selected.IsUserDefined)
                .DistinctUntilChanged();


            var inlineViewerVisible = isUserDefinedChanged.CombineLatest(this.WhenValueChanged(vm => vm.ShowInline),
                                                            (userDefined, showInline) => userDefined && showInline);


            CanViewInline = isUserDefinedChanged.ForBinding();
            InlineViewerVisible = inlineViewerVisible.ForBinding();

            //return an empty line provider unless user is viewing inline - this saves needless trips to the file
            var inline = indexed.CombineLatest(inlineViewerVisible, (index, ud) => ud ? index : new EmptyLineProvider());
            InlineViewer = inlineViewerFactory.Create(inline, this.WhenValueChanged(vm => vm.SelectedItem));

            _cleanUp = new CompositeDisposable(lineScroller,
                loader,
                firstIndexMonitor,
                IsLoading,
                Count,
                LatestCount,
                FileSizeText,
                SearchHint,
                ShouldHightlightMatchingText,
                SelectedItemsCount,
                CanViewInline,
                InlineViewer,
                InlineViewerVisible,
                SearchCollection,
                searchInfoCollection,
                Disposable.Create(() =>
                {
                    _userScrollRequested.OnCompleted();
                    (SelectionMonitor as IDisposable)?.Dispose();
                }));
        }



        public LineProxy SelectedItem
        {
            get { return _selectedLine; }
            set { SetAndRaise(ref _selectedLine, value);}
        }

        void IScrollReceiver.ScrollBoundsChanged(ScrollBoundsArgs boundsArgs)
        {
            if (boundsArgs == null) throw new ArgumentNullException(nameof(boundsArgs));
            var mode = AutoTail ? ScrollReason.Tail : ScrollReason.User;

            /*
                I need to get rid of this subject as I prefer functional over imperative. 
                However due to complexities int the interactions with the VirtualScrollPanel,
                each time I have tried to remove it all hell has broken loose
            */
            _userScrollRequested.OnNext(new ScrollRequest(mode, boundsArgs.PageSize,boundsArgs.FirstIndex));
            PageSize = boundsArgs.PageSize;
            FirstIndex = boundsArgs.FirstIndex;
        }

        void IScrollReceiver.ScrollChanged(ScrollChangedArgs scrollChangedArgs)
        {
            if (scrollChangedArgs.Direction == ScrollDirection.Up)
                AutoTail = false;

        }
        public string SearchText
        {
            get { return _searchText; }
            set { SetAndRaise(ref _searchText, value); }
        }

        public bool AutoTail
        {
            get { return _autoTail; }
            set { SetAndRaise(ref _autoTail, value); }
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

        public bool ShowInline
        {
            get { return _showInline; }
            set { SetAndRaise(ref _showInline, value); }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
