using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using DynamicData.PLinq;
using MaterialDesignThemes.Wpf;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Infrastucture;
using TailBlazer.Settings;
using TailBlazer.Views.Formatting;

namespace TailBlazer.Views
{
    public class FileTailerViewModel: AbstractNotifyPropertyChanged, IDisposable, IScrollReceiver
    {
        private readonly IDisposable _cleanUp;
        private readonly ReadOnlyObservableCollection<LineProxy> _data;
        private readonly ISubject<ScrollRequest> _userScrollRequested = new ReplaySubject<ScrollRequest>(1);

        private bool _autoTail=true;
        private int _firstIndex;
        private int _pageSize;
        private LineProxy _selectedLine;
        private bool _showInline;

        public ReadOnlyObservableCollection<LineProxy> Lines => _data;

        public Guid Id { get; }= Guid.NewGuid();
        public ICommand CopyToClipboardCommand { get; }
        public ICommand AddSearchCommand { get; }
        public ISelectionMonitor SelectionMonitor { get; }
        public SearchOptionsViewModel SearchOptions { get;  }

        public SearchHints SearchHints { get;  }
        public SearchCollection SearchCollection { get; }
        public InlineViewer InlineViewer { get; }
        public IProperty<int> SelectedItemsCount { get; }
        public IProperty<string> SearchHint { get; }
        public IProperty<int> Count { get; }
        public IProperty<string> CountText { get; }
        public IProperty<int> LatestCount { get; }
        public IProperty<string> FileSizeText { get; }
        public IProperty<bool> IsLoading { get; }
        public IProperty<bool> InlineViewerVisible { get; }
        public IProperty<bool> CanViewInline { get; }
        public IProperty<bool> HighlightTail { get; }
        public IProperty<bool> UsingDarkTheme { get; }
        public IProperty<Duration> HighlightDuration { get; }

        public ICommand OpenSearchOptionsCommand => new Command(OpenSearchOptions);

        public FileTailerViewModel([NotNull] ILogger logger,
            [NotNull] ISchedulerProvider schedulerProvider,
            [NotNull] IFileWatcher fileWatcher,
            [NotNull] ISelectionMonitor selectionMonitor, 
            [NotNull] IClipboardHandler clipboardHandler, 
            [NotNull] ISearchInfoCollection searchInfoCollection, 
            [NotNull] IInlineViewerFactory inlineViewerFactory, 
            [NotNull] ISetting<GeneralOptions> generalOptions,
            [NotNull] IRecentSearchCollection recentSearchCollection, 
            [NotNull] ISearchMetadataCollection searchMetadataCollection,
            [NotNull] SearchOptionsViewModel searchOptionsViewModel,
            [NotNull] SearchHints searchHints)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            if (fileWatcher == null) throw new ArgumentNullException(nameof(fileWatcher));
            if (selectionMonitor == null) throw new ArgumentNullException(nameof(selectionMonitor));
            if (clipboardHandler == null) throw new ArgumentNullException(nameof(clipboardHandler));
            if (searchInfoCollection == null) throw new ArgumentNullException(nameof(searchInfoCollection));
            if (inlineViewerFactory == null) throw new ArgumentNullException(nameof(inlineViewerFactory));
            if (generalOptions == null) throw new ArgumentNullException(nameof(generalOptions));
            if (searchMetadataCollection == null) throw new ArgumentNullException(nameof(searchMetadataCollection));
            if (searchOptionsViewModel == null) throw new ArgumentNullException(nameof(searchOptionsViewModel));
            if (searchHints == null) throw new ArgumentNullException(nameof(searchHints));

            SelectionMonitor = selectionMonitor;
            SearchOptions = searchOptionsViewModel;
            SearchHints = searchHints;
            CopyToClipboardCommand = new Command(()=> clipboardHandler.WriteToClipboard(selectionMonitor.GetSelectedText()));
            SelectedItemsCount = selectionMonitor.Selected.Connect().QueryWhenChanged(collection=> collection.Count).ForBinding();
            SearchCollection = new SearchCollection(searchInfoCollection, schedulerProvider);

            UsingDarkTheme = generalOptions.Value
                    .ObserveOn(schedulerProvider.MainThread)
                    .Select(options => options.Theme== Theme.Dark)
                    .ForBinding();

            HighlightTail = generalOptions.Value
                .ObserveOn(schedulerProvider.MainThread)
                .Select(options => options.HighlightTail)
                .ForBinding();

            HighlightDuration = generalOptions.Value
                .ObserveOn(schedulerProvider.MainThread)
                .Select(options => new Duration(TimeSpan.FromSeconds(options.HighlightDuration)))
                .ForBinding();

            //An observable which acts as a scroll command
            var autoChanged = this.WhenValueChanged(vm => vm.AutoTail);
            var scroller = _userScrollRequested.CombineLatest(autoChanged, (user, auto) =>
                        {
                            var mode = AutoTail ? ScrollReason.Tail : ScrollReason.User;
                            return  new ScrollRequest(mode, user.PageSize, user.FirstIndex);
                        })
                        .ObserveOn(schedulerProvider.Background)
                        .DistinctUntilChanged();

            IsLoading = searchInfoCollection.All.Take(1).Select(_ => false).StartWith(true).ForBinding();
            
            //command to add the current search to the tail collection
            AddSearchCommand = new Command(() =>
            {
                var text = SearchHints.SearchText;
                searchInfoCollection.Add(text);
                recentSearchCollection.Add(new RecentSearch(text));
                SearchHints.SearchText = string.Empty;

            },()=> SearchHints.SearchText.IsLongerThanOrEqualTo(3));

            //User feedback to show file size
            FileSizeText = fileWatcher.Latest.Select(fn=>fn.Size)
                .Select(size => size.FormatWithAbbreviation())
                .DistinctUntilChanged()
                .ForBinding();

            //User feedback to guide them whilst typing
            SearchHint = this.WhenValueChanged(vm => vm.SearchHints.SearchText)
                            .Select(text =>
                            {
                                if (string.IsNullOrEmpty(text)) return "Type to search";
                                return text.Length < 3 ? "Enter at least 3 characters" : "Hit enter to search";
                            }).ForBinding();

            //tailer is the main object used to tail, scroll and filter in a file
            var lineScroller = new LineScroller(SearchCollection.Latest.ObserveOn(schedulerProvider.Background), scroller);

            //load lines into observable collection
            var lineProxyFactory = new LineProxyFactory(new TextFormatter(searchMetadataCollection));
            var loader = lineScroller.Lines.Connect()
                .Transform(x=>lineProxyFactory.Create(x), new ParallelisationOptions(ParallelType.Ordered,5))
                .Sort(SortExpressionComparer<LineProxy>.Ascending(proxy => proxy))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _data)
                .DisposeMany()
                .Subscribe(changes => logger.Info($"Rows changed. {changes.Adds} adds, {changes.Removes} removed"), 
                            ex => logger.Error(ex, "There is a problem with bind data"));
            
            //monitor matching lines and start index,
            Count = searchInfoCollection.All.Select(latest=>latest.Count).ForBinding();
            CountText = searchInfoCollection.All.Select(latest => $"{latest.Count.ToString("##,###")} lines").ForBinding();
            LatestCount = SearchCollection.Latest.Select(latest => latest.Count).ForBinding();
            
            //track first visible index
            var firstIndexMonitor = lineScroller.Lines.Connect()
                .Buffer(TimeSpan.FromMilliseconds(250)).FlattenBufferResult()
                .ToCollection()
                .Select(lines => lines.Count == 0 ? 0 : lines.Select(l => l.Index).Max() - lines.Count+1)
                .ObserveOn(schedulerProvider.MainThread)
                .Subscribe(first => FirstIndex = first);

            //Create objects required for inline viewing
            var isUserDefinedChanged = SearchCollection.WhenValueChanged(sc => sc.Selected)
                .Where(selected=> selected!=null)
                .Select(selected => selected.IsUserDefined)
                .DistinctUntilChanged()
                .Replay(1)
                .RefCount();
            
            var inlineViewerVisible = isUserDefinedChanged.CombineLatest(this.WhenValueChanged(vm => vm.ShowInline),
                                                            (userDefined, showInline) => userDefined && showInline);
            
            CanViewInline = isUserDefinedChanged.ForBinding();
            InlineViewerVisible = inlineViewerVisible.ForBinding();

            //return an empty line provider unless user is viewing inline - this saves needless trips to the file
            var inline = searchInfoCollection.All.CombineLatest(inlineViewerVisible, (index, ud) => ud ? index : new EmptyLineProvider());
            InlineViewer = inlineViewerFactory.Create(inline, this.WhenValueChanged(vm => vm.SelectedItem),lineProxyFactory);

            _cleanUp = new CompositeDisposable(lineScroller,
                loader,
                firstIndexMonitor,
                IsLoading,
                Count,
                LatestCount,
                FileSizeText,
                SearchHint,
                SelectedItemsCount,
                CanViewInline,
                InlineViewer,
                InlineViewerVisible,
                SearchCollection,
                searchInfoCollection,
                HighlightTail,
                UsingDarkTheme,
                searchHints,
                searchMetadataCollection,
                searchMetadataCollection,
                SelectionMonitor,
                SearchOptions,
                Disposable.Create(_userScrollRequested.OnCompleted)
                );
        }
        
        void IScrollReceiver.ScrollBoundsChanged(ScrollBoundsArgs boundsArgs)
        {
            if (boundsArgs == null) throw new ArgumentNullException(nameof(boundsArgs));
            var mode = AutoTail ? ScrollReason.Tail : ScrollReason.User;

            PageSize = boundsArgs.PageSize;
            FirstIndex = boundsArgs.FirstIndex;

            /*
                I need to get rid of this subject as I prefer functional over imperative. 
                However due to complexities int the interactions with the VirtualScrollPanel,
                each time I have tried to remove it all hell has broken loose
            */
            _userScrollRequested.OnNext(new ScrollRequest(mode, boundsArgs.PageSize,boundsArgs.FirstIndex));
 
        }

        public async void OpenSearchOptions()
        {
            var view = new SearchOptionsView
            {
                DataContext = this.SearchOptions
            };

            //show the dialog
           var result =  await DialogHost.Show(view, Id);
        }

        void IScrollReceiver.ScrollChanged(ScrollChangedArgs scrollChangedArgs)
        {
            if (scrollChangedArgs.Direction == ScrollDirection.Up)
                AutoTail = false;
        }

        public LineProxy SelectedItem
        {
            get { return _selectedLine; }
            set { SetAndRaise(ref _selectedLine, value); }
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
