using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using DynamicData.PLinq;
using MaterialDesignThemes.Wpf;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Domain.StateHandling;
using TailBlazer.Infrastucture;
using TailBlazer.Infrastucture.Virtualisation;
using TailBlazer.Views.Options;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Tail
{
    public class TailViewModel: AbstractNotifyPropertyChanged, ILinesVisualisation, IPersistentView
    {
        private readonly IDisposable _cleanUp;
        private readonly SingleAssignmentDisposable _stateMonitor= new SingleAssignmentDisposable();
        private readonly ReadOnlyObservableCollection<LineProxy> _data;
        private readonly ISubject<ScrollRequest> _userScrollRequested = new ReplaySubject<ScrollRequest>(1);
        private readonly IPersistentView _persister;
        private readonly ITailViewStateControllerFactory _tailViewStateControllerFactory;

        private bool _autoTail=true;
        private int _firstIndex;
        private int _pageSize;
        private LineProxy _selectedLine;
        private bool _showInline;

        public ReadOnlyObservableCollection<LineProxy> Lines => _data;
        public Guid Id { get; }= Guid.NewGuid();
        public ISelectionMonitor SelectionMonitor { get; }
        private SearchOptionsViewModel SearchOptions { get;  }
        public SearchHints SearchHints { get;  }
        public SearchCollection SearchCollection { get; }
        internal ISearchMetadataCollection SearchMetadataCollection { get; }
        public InlineViewer InlineViewer { get; }
        public IProperty<int> Count { get; }
        public IProperty<string> CountText { get; }
        public IProperty<int> LatestCount { get; }
        public IProperty<string> FileSizeText { get; }
        public IProperty<FileStatus> FileStatus { get; }
        public IProperty<bool> InlineViewerVisible { get; }
        public IProperty<bool> CanViewInline { get; }
        public IProperty<bool> HighlightTail { get; }
        public IProperty<bool> UsingDarkTheme { get; }
        public IProperty<int> MaximumChars { get; }
        public ICommand CopyToClipboardCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand CopyPathToClipboardCommand { get; }
        public ICommand OpenSearchOptionsCommand => new Command(OpenSearchOptions);

        public string Name { get; }

        public TailViewModel([NotNull] ILogger logger,
            [NotNull] ISchedulerProvider schedulerProvider,
            [NotNull] IFileWatcher fileWatcher,
            [NotNull] ISelectionMonitor selectionMonitor, 
            [NotNull] IClipboardHandler clipboardHandler, 
            [NotNull] ISearchInfoCollection searchInfoCollection, 
            [NotNull] IInlineViewerFactory inlineViewerFactory, 
            [NotNull] ISetting<GeneralOptions> generalOptions,
            [NotNull] ISearchMetadataCollection searchMetadataCollection,
            [NotNull] IStateBucketService stateBucketService,
            [NotNull] SearchOptionsViewModel searchOptionsViewModel,
            [NotNull] ITailViewStateRestorer restorer,
            [NotNull] SearchHints searchHints,
            [NotNull] ITailViewStateControllerFactory tailViewStateControllerFactory,
            [NotNull] IThemeProvider themeProvider)
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
            if (stateBucketService == null) throw new ArgumentNullException(nameof(stateBucketService));
            if (searchOptionsViewModel == null) throw new ArgumentNullException(nameof(searchOptionsViewModel));
            if (searchHints == null) throw new ArgumentNullException(nameof(searchHints));
            if (themeProvider == null) throw new ArgumentNullException(nameof(themeProvider));

            Name = fileWatcher.FullName;
            SelectionMonitor = selectionMonitor;
            SearchOptions = searchOptionsViewModel;
            SearchHints = searchHints;
            SearchCollection = new SearchCollection(searchInfoCollection, schedulerProvider);
            CopyToClipboardCommand = new Command(() => clipboardHandler.WriteToClipboard(selectionMonitor.GetSelectedText()));
            OpenFileCommand = new Command(() => Process.Start(fileWatcher.FullName));
            OpenFolderCommand = new Command(() => Process.Start(fileWatcher.Folder));
            CopyPathToClipboardCommand = new Command(() => clipboardHandler.WriteToClipboard(fileWatcher.FullName));
            SearchMetadataCollection = searchMetadataCollection;
            
            var horizonalScrollArgs = new ReplaySubject<TextScrollInfo>(1);
            HorizonalScrollChanged = args =>
            {
                horizonalScrollArgs.OnNext(args);
            };

            _tailViewStateControllerFactory = tailViewStateControllerFactory;
            
            //Move these 2 highlight fields to a service as all views require them
            UsingDarkTheme = generalOptions.Value
                    .ObserveOn(schedulerProvider.MainThread)
                    .Select(options => options.Theme== Theme.Dark)
                    .ForBinding();

            HighlightTail = generalOptions.Value
                .ObserveOn(schedulerProvider.MainThread)
                .Select(options => options.HighlightTail)
                .ForBinding();

            //this deals with state when loading the system at start up and at shut-down
            _persister = new TailViewPersister(this, restorer);
            
            //An observable which acts as a scroll command
            var autoChanged = this.WhenValueChanged(vm => vm.AutoTail);
            var scroller = _userScrollRequested.CombineLatest(autoChanged, (user, auto) =>
                        {
                            var mode = AutoTail ? ScrollReason.Tail : ScrollReason.User;
                            return  new ScrollRequest(mode, user.PageSize, user.FirstIndex);
                        })
                        .Do(x=>logger.Info("Scrolling to {0}/{1}", x.FirstIndex,x.PageSize))
                        .DistinctUntilChanged();

            FileStatus = fileWatcher.Status.ForBinding();

            //command to add the current search to the tail collection
            var searchInvoker = SearchHints.SearchRequested.Subscribe(request =>
            {
                searchInfoCollection.Add(request.Text, request.UseRegEx);
            });

            //User feedback to show file size
            FileSizeText = fileWatcher.Latest.Select(fn=>fn.Size)
                .Select(size => size.FormatWithAbbreviation())
                .DistinctUntilChanged()
                .ForBinding();

            //tailer is the main object used to tail, scroll and filter in a file
            var lineScroller = new LineScroller(SearchCollection.Latest.ObserveOn(schedulerProvider.Background), scroller);
            
            MaximumChars = lineScroller.MaximumLines()
                            .ObserveOn(schedulerProvider.MainThread)
                            .ForBinding();

            //load lines into observable collection
            var lineProxyFactory = new LineProxyFactory(new TextFormatter(searchMetadataCollection), new LineMatches(searchMetadataCollection), horizonalScrollArgs.DistinctUntilChanged(), themeProvider);

            var loader = lineScroller.Lines.Connect()
                .LogChanges(logger, "Received")
                .Transform(lineProxyFactory.Create, new ParallelisationOptions(ParallelType.Ordered, 3))
                .LogChanges(logger, "Sorting")
                .Sort(SortExpressionComparer<LineProxy>.Ascending(proxy => proxy))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _data,100)
                .LogChanges(logger, "Bound")
                .DisposeMany()
                .LogErrors(logger)
                .Subscribe();
            
            //monitor matching lines and start index,
            Count = searchInfoCollection.All.Select(latest=>latest.Count).ForBinding();
            CountText = searchInfoCollection.All.Select(latest => $"{latest.Count.ToString("##,###")} lines").ForBinding();
            LatestCount = SearchCollection.Latest.Select(latest => latest.Count).ForBinding();

            ////track first visible index
            var firstIndexMonitor = lineScroller.Lines.Connect()
                .Buffer(TimeSpan.FromMilliseconds(25)).FlattenBufferResult()
                .ToCollection()
                .Select(lines => lines.Count == 0 ? 0 : lines.Select(l => l.Index).Max() - lines.Count + 1)
                .ObserveOn(schedulerProvider.MainThread)
                .Subscribe(first =>
                {
                    FirstIndex = first;
                });


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

            InlineViewer = inlineViewerFactory.Create(inline, this.WhenValueChanged(vm => vm.SelectedItem), searchMetadataCollection);

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
                SelectionMonitor,
                SearchOptions,
                searchInvoker,
                MaximumChars,
                _stateMonitor,
                horizonalScrollArgs.SetAsComplete(),
                _userScrollRequested.SetAsComplete());
        }

     
        public TextScrollDelegate HorizonalScrollChanged { get; }


        private async void OpenSearchOptions()
        {
           await DialogHost.Show(SearchOptions, Id);
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
            if (scrollChangedArgs.Direction == ScrollDirection.Up)
                AutoTail = false;
        }

        void IScrollReceiver.ScrollDiff(int linesChanged)
        {
            _userScrollRequested.OnNext(new ScrollRequest(ScrollReason.User, PageSize, FirstIndex + linesChanged));
        }

        #endregion

        #region Persist state 

        public void ApplySettings()
        {
            //this controller responsible for loading and persisting user search stuff as the user changes stuff
            _stateMonitor.Disposable = _tailViewStateControllerFactory.Create(this,true);
        }


        ViewState IPersistentView.CaptureState()
        {
            return _persister.CaptureState();
        }

        void IPersistentView.Restore(ViewState state)
        {
            //When this is called, we assume that FileInfo has not been set!
            _persister.Restore(state);

            //this controller responsible for loading and persisting user search stuff as the user changes stuff
            _stateMonitor.Disposable = _tailViewStateControllerFactory.Create(this,false);
        }

        #endregion
        
        public void Dispose()
        {
            _cleanUp.Dispose();
        }

    }
}
