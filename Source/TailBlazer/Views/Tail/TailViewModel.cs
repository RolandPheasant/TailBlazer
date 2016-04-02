using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using TailBlazer.Controls;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Infrastucture;
using TailBlazer.Views.Options;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Tail
{
    public class TailViewModel : AbstractNotifyPropertyChanged, ILinesVisualisation, IPersistentStateProvider
    {
        private readonly IDisposable _cleanUp;
        private readonly ReadOnlyObservableCollection<LineProxy> _data;
        private readonly IPersistentStateProvider _stateProvider;
        private readonly ISubject<ScrollRequest> _userScrollRequested = new ReplaySubject<ScrollRequest>(1);
        private bool _autoTail = true;
        private int _firstIndex;
        private int _pageSize;
        private LineProxy _selectedLine;
        private bool _showInline;

        public TailViewModel([NotNull] ILogger logger,
            [NotNull] ISchedulerProvider schedulerProvider,
            [NotNull] IEnumerable<IFileWatcher> fileWatcher,
            [NotNull] ISelectionMonitor selectionMonitor,
            [NotNull] IClipboardHandler clipboardHandler,
            [NotNull] ISearchInfoCollection searchInfoCollection,
            [NotNull] IInlineViewerFactory inlineViewerFactory,
            [NotNull] ISetting<GeneralOptions> generalOptions,
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

            _stateProvider = new TailViewPersister(this);

            var enumerable = fileWatcher as IFileWatcher[] ?? fileWatcher.ToArray();
            Names = enumerable.Select(t => t.FullName);
            SelectionMonitor = selectionMonitor;
            SearchOptions = searchOptionsViewModel;
            SearchHints = searchHints;
            SearchCollection = new SearchCollection(searchInfoCollection, schedulerProvider);

            CopyToClipboardCommand =
                new Command(() => clipboardHandler.WriteToClipboard(selectionMonitor.GetSelectedText()));
            OpenFileCommand = new Command(() => Process.Start(enumerable[0].FullName));
            OpenFolderCommand = new Command(() => Process.Start(enumerable[0].Folder));

            UsingDarkTheme = generalOptions.Value
                .ObserveOn(schedulerProvider.MainThread)
                .Select(options => options.Theme == Theme.Dark)
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
                return new ScrollRequest(mode, user.PageSize, user.FirstIndex);
            })
                .Do(x => logger.Info("Scrolling to {0}/{1}", x.FirstIndex, x.PageSize))
                .DistinctUntilChanged();

            FileStatus = enumerable
                .Select(t => t.Status)
                .Merge()
                .Scan(default(FileStatus), (status, fileStatus) => status | fileStatus)
                .ForBinding();

            //command to add the current search to the tail collection
            var searchInvoker =
                SearchHints.SearchRequested.Subscribe(
                    request => { searchInfoCollection.Add(request.Text, request.UseRegEx); });

            //User feedback to show file size
            FileSizeText = enumerable
                .Select(t => t.Latest)
                .Merge()
                .Select(t => t.Size)
                .Scan(0f, (previousSize, currentSize) => previousSize + currentSize/2f)
                .Select(t => ((long)t).FormatWithAbbreviation())
                .DistinctUntilChanged()
                .ForBinding();


            //tailer is the main object used to tail, scroll and filter in a file
            var lineScroller = new LineScroller(SearchCollection.Latest.ObserveOn(schedulerProvider.Background),
                scroller);

            //load lines into observable collection
            var lineProxyFactory = new LineProxyFactory(new TextFormatter(searchMetadataCollection),
                new LineMatches(searchMetadataCollection));

            var loader = lineScroller.Lines.Connect()
                .RecordChanges(logger, "Received")
                .Transform(lineProxyFactory.Create, new ParallelisationOptions(ParallelType.Ordered, 3))
                .Sort(SortExpressionComparer<LineProxy>.Ascending(proxy => proxy))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _data, 100)
                .DisposeMany()
                .LogErrors(logger)
                .Subscribe();


            //monitor matching lines and start index,
            Count = searchInfoCollection.All
                .GroupBy(t => t)
                .Scan(0, (i, provider) => i + provider.Key.Count)
                .ForBinding();
            CountText = searchInfoCollection.All
                .GroupBy(t => t)
                .Scan(0L, (i, provider) => i + provider.Key.Count)
                .Select(latestCount => $"{latestCount.ToString("##,###")} lines").ForBinding();
            LatestCount = SearchCollection.Latest
                .GroupBy(t => t)
                .Scan(0, (i, provider) => i + provider.Key.Count)
                .ForBinding();

            ////track first visible index
            var firstIndexMonitor = lineScroller.Lines.Connect()
                .Buffer(TimeSpan.FromMilliseconds(25)).FlattenBufferResult()
                .ToCollection()
                .Select(lines => lines.Count == 0 ? 0 : lines.Select(l => l.Index).Max() - lines.Count + 1)
                .ObserveOn(schedulerProvider.MainThread)
                .Subscribe(first => { FirstIndex = first; });

            //Create objects required for inline viewing
            var isUserDefinedChanged = SearchCollection.WhenValueChanged(sc => sc.Selected)
                .Where(selected => selected != null)
                .Select(selected => selected.IsUserDefined)
                .DistinctUntilChanged()
                .Replay(1)
                .RefCount();

            var inlineViewerVisible = isUserDefinedChanged.CombineLatest(this.WhenValueChanged(vm => vm.ShowInline),
                (userDefined, showInline) => userDefined && showInline);

            CanViewInline = isUserDefinedChanged.ForBinding();
            InlineViewerVisible = inlineViewerVisible.ForBinding();

            //return an empty line provider unless user is viewing inline - this saves needless trips to the file

            var inline = searchInfoCollection.All.CombineLatest(inlineViewerVisible,
                (index, ud) => ud ? index : new EmptyLineProvider());

            var firstVisibleRow = _data.ToObservableChangeSet().ToCollection()
                .Select(collection => collection.FirstOrDefault());

            //var itemToSelect = this.WhenValueChanged(vm => vm.SelectedItem)
            //    .CombineLatest(firstVisibleRow, (selected, first) => selected ?? first);
            ////
            InlineViewer = inlineViewerFactory.Create(inline, this.WhenValueChanged(vm => vm.SelectedItem),
                lineProxyFactory);

            _cleanUp = new CompositeDisposable(lineScroller,
                loader,
                firstIndexMonitor,
                FileStatus,
                Count,
                LatestCount,
                FileSizeText,
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
                searchInvoker,
                _userScrollRequested.SetAsComplete());
        }

        public Guid Id { get; } = Guid.NewGuid();
        public ICommand OpenFileCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public SearchOptionsViewModel SearchOptions { get; }
        public SearchHints SearchHints { get; }
        public SearchCollection SearchCollection { get; }
        public InlineViewer InlineViewer { get; }
        public IProperty<string> CountText { get; }
        public IProperty<int> LatestCount { get; }
        public IProperty<string> FileSizeText { get; private set; }
        public IProperty<FileStatus> FileStatus { get; private set; }
        public IProperty<bool> InlineViewerVisible { get; }
        public IProperty<bool> CanViewInline { get; }
        public IProperty<bool> HighlightTail { get; }
        public IProperty<bool> UsingDarkTheme { get; }
        public IProperty<Duration> HighlightDuration { get; }
        public ICommand OpenSearchOptionsCommand => new Command(OpenSearchOptions);
        public IEnumerable<string> Names { get; }

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

        public bool ShowInline
        {
            get { return _showInline; }
            set { SetAndRaise(ref _showInline, value); }
        }

        public ReadOnlyObservableCollection<LineProxy> Lines => _data;
        public ICommand CopyToClipboardCommand { get; }
        public ISelectionMonitor SelectionMonitor { get; }
        public IProperty<int> Count { get; }

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

        private async void OpenSearchOptions()
        {
            await DialogHost.Show(SearchOptions, Id);
        }

        #region Interact with scroll panel

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
            _userScrollRequested.OnNext(new ScrollRequest(mode, boundsArgs.PageSize, boundsArgs.FirstIndex));
        }

        void IScrollReceiver.ScrollChanged(ScrollChangedArgs scrollChangedArgs)
        {
            AutoTail &= scrollChangedArgs.Direction != ScrollDirection.Up;
        }

        void IScrollReceiver.ScrollDiff(int linesChanged)
        {
            _userScrollRequested.OnNext(new ScrollRequest(ScrollReason.User, PageSize, FirstIndex + linesChanged));
        }

        #endregion

        #region Persist state 

        State IPersistentStateProvider.CaptureState()
        {
            return _stateProvider.CaptureState();
        }

        void IPersistentStateProvider.Restore(State state)
        {
            _stateProvider.Restore(state);
        }

        #endregion
    }
}