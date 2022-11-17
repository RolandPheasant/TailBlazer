using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Domain.StateHandling;
using TailBlazer.Infrastructure;
using TailBlazer.Infrastructure.KeyboardNavigation;
using TailBlazer.Infrastructure.Virtualisation;
using TailBlazer.Views.DialogServices;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Tail;

public class TailViewModel: AbstractNotifyPropertyChanged, ILinesVisualisation, IPersistentView, IDialogViewModel, IPageProvider, ISelectedAware
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
    private bool _isDialogOpen;
    private object _dialogContent;
    private bool _isSelected;

    public ReadOnlyObservableCollection<LineProxy> Lines => _data;
    public Guid Id { get; } = Guid.NewGuid();
    public ISelectionMonitor SelectionMonitor { get; }
    public GeneralOptionBindings GeneralOptionBindings { get;  }
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

    public IProperty<int> MaximumChars { get; }

    public ICommand CopyToClipboardCommand { get; }
    public ICommand OpenFileCommand { get; }
    public ICommand OpenFolderCommand { get; }
    public ICommand CopyPathToClipboardCommand { get; }
    public ICommand OpenSearchOptionsCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand UnClearCommand { get; }
    public ICommand KeyAutoTail { get; }
    public string Name { get; }



    public TailViewModel([NotNull] ILogger logger,
        [NotNull] ISchedulerProvider schedulerProvider,
        [NotNull] IFileWatcher fileWatcher,
        [NotNull] ISelectionMonitor selectionMonitor, 
        [NotNull] IClipboardHandler clipboardHandler, 
        [NotNull] ISearchInfoCollection searchInfoCollection, 
        [NotNull] IInlineViewerFactory inlineViewerFactory,
        [NotNull] GeneralOptionBindings generalOptionBindings,
        [NotNull] ICombinedSearchMetadataCollection combinedSearchMetadataCollection,
        [NotNull] IStateBucketService stateBucketService,
        [NotNull] ITailViewStateRestorer restorer,
        [NotNull] SearchHints searchHints,
        [NotNull] ITailViewStateControllerFactory tailViewStateControllerFactory,
        [NotNull] IThemeProvider themeProvider,
        [NotNull] SearchCollection searchCollection, 
        [NotNull] ITextFormatter textFormatter,
        [NotNull] ILineMatches lineMatches,
        [NotNull] IObjectProvider objectProvider,
        [NotNull] IDialogCoordinator dialogCoordinator)
    {
         
        if (logger == null) throw new ArgumentNullException(nameof(logger));
        if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
        if (fileWatcher == null) throw new ArgumentNullException(nameof(fileWatcher));
        if (clipboardHandler == null) throw new ArgumentNullException(nameof(clipboardHandler));
        if (searchInfoCollection == null) throw new ArgumentNullException(nameof(searchInfoCollection));
        if (inlineViewerFactory == null) throw new ArgumentNullException(nameof(inlineViewerFactory));
        if (stateBucketService == null) throw new ArgumentNullException(nameof(stateBucketService));
        if (themeProvider == null) throw new ArgumentNullException(nameof(themeProvider));
        if (textFormatter == null) throw new ArgumentNullException(nameof(textFormatter));
        if (lineMatches == null) throw new ArgumentNullException(nameof(lineMatches));
        if (objectProvider == null) throw new ArgumentNullException(nameof(objectProvider));
        if (dialogCoordinator == null) throw new ArgumentNullException(nameof(dialogCoordinator));
        if (combinedSearchMetadataCollection == null) throw new ArgumentNullException(nameof(combinedSearchMetadataCollection));

        Name = fileWatcher.FullName;
        SelectionMonitor = selectionMonitor ?? throw new ArgumentNullException(nameof(selectionMonitor));
        GeneralOptionBindings = generalOptionBindings;
        SearchHints = searchHints ?? throw new ArgumentNullException(nameof(searchHints));

        CopyToClipboardCommand = new Command(() => clipboardHandler.WriteToClipboard(selectionMonitor.GetSelectedText()));
        OpenFileCommand = new Command(() => Process.Start(fileWatcher.FullName));
        OpenFolderCommand = new Command(() => Process.Start(fileWatcher.Folder));
        CopyPathToClipboardCommand = new Command(() => clipboardHandler.WriteToClipboard(fileWatcher.FullName));
        UnClearCommand = new Command(fileWatcher.Reset);
        ClearCommand = new Command(fileWatcher.Clear);
        KeyAutoTail = new Command(() => { AutoTail = true; });
        OpenSearchOptionsCommand = new Command(async () =>
        {
            await Task.Run(() =>
            {
                var content = objectProvider.Get<SearchOptionsViewModel>(new Argument<ICombinedSearchMetadataCollection>(combinedSearchMetadataCollection));
                dialogCoordinator.Show(this, content, x => content.Dispose());
            });
        });

        var closeOnDeselect = this.WhenValueChanged(vm => vm.IsSelected, false)
            .Where(selected => !selected)
            .Subscribe(_ => dialogCoordinator.Close());

        SearchCollection = searchCollection ?? throw new ArgumentNullException(nameof(searchCollection));
        SearchMetadataCollection = combinedSearchMetadataCollection.Local;

        var horizonalScrollArgs = new ReplaySubject<TextScrollInfo>(1);
        HorizonalScrollChanged = args => horizonalScrollArgs.OnNext(args);
            
        _tailViewStateControllerFactory = tailViewStateControllerFactory;

        //this deals with state when loading the system at start up and at shut-down
        _persister = new TailViewPersister(this, restorer);

        FileStatus = fileWatcher.Status.ForBinding();

        //command to add the current search to the tail collection
        var searchInvoker = SearchHints.SearchRequested.Subscribe(request => searchInfoCollection.Add(request.Text, request.UseRegEx));

        //An observable which acts as a scroll command
        var autoChanged = this.WhenValueChanged(vm => vm.AutoTail);
        var scroller = _userScrollRequested.CombineLatest(autoChanged, (user, auto) =>
            {
                var mode = AutoTail ? ScrollReason.Tail : ScrollReason.User;
                return new ScrollRequest(mode, user.PageSize, user.FirstIndex);
            })
            .Do(x => logger.Info("Scrolling to {0}/{1}", x.FirstIndex, x.PageSize))
            .DistinctUntilChanged();

        //User feedback to show file size
        FileSizeText = fileWatcher.Latest.Select(fn=>fn.Size)
            .Select(size => size.FormatWithAbbreviation())
            .DistinctUntilChanged()
            .ForBinding();


        //tailer is the main object used to tail, scroll and filter in a file
        var selectedProvider = SearchCollection.Latest.ObserveOn(schedulerProvider.Background);
            
        var lineScroller = new LineScroller(selectedProvider, scroller);
            
        MaximumChars = lineScroller.MaximumLines()
            .ObserveOn(schedulerProvider.MainThread)
            .ForBinding();

        var lineProxyFactory = new LineProxyFactory(textFormatter, lineMatches, horizonalScrollArgs.DistinctUntilChanged(), themeProvider);

        var loader = lineScroller.Lines.Connect()
            .LogChanges(logger, "Received")
            .Transform(lineProxyFactory.Create)
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
        CountText = searchInfoCollection.All.Select(latest => $"{latest.Count:##,###} lines").ForBinding();
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

        var showInline = this.WhenValueChanged(vm => vm.ShowInline);
        var inlineViewerVisible = isUserDefinedChanged.CombineLatest(showInline, (userDefined, showInlne) => userDefined && showInlne);
            
        CanViewInline = isUserDefinedChanged.ForBinding();
        InlineViewerVisible = inlineViewerVisible.ForBinding();

        //return an empty line provider unless user is viewing inline - this saves needless trips to the file
        var inline = searchInfoCollection.All.CombineLatest(inlineViewerVisible, (index, ud) => ud ? index : EmptyLineProvider.Instance);

        InlineViewer = inlineViewerFactory. Create(combinedSearchMetadataCollection, inline, this.WhenValueChanged(vm => vm.SelectedItem));

        _cleanUp = new CompositeDisposable(lineScroller,
            loader,
            firstIndexMonitor,
            FileStatus,
            Count,
            CountText,
            LatestCount,
            FileSizeText,
            CanViewInline,
            InlineViewer,
            InlineViewerVisible,
            SearchCollection,
            searchInfoCollection,
            searchHints,
            SelectionMonitor,
            closeOnDeselect,
            Disposable.Create(dialogCoordinator.Close),
            searchInvoker,
            MaximumChars,
            _stateMonitor,
            combinedSearchMetadataCollection,
            horizonalScrollArgs.SetAsComplete(),
            _userScrollRequested.SetAsComplete());
    }
     
    public TextScrollDelegate HorizonalScrollChanged { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetAndRaise(ref _isSelected, value);
    }

    public bool IsDialogOpen
    {
        get => _isDialogOpen;
        set => SetAndRaise(ref _isDialogOpen, value);
    }

    public object DialogContent
    {
        get => _dialogContent;
        set => SetAndRaise(ref _dialogContent, value);
    }

    public LineProxy SelectedItem
    {
        get => _selectedLine;
        set => SetAndRaise(ref _selectedLine, value);
    }

    public bool AutoTail
    {
        get => _autoTail;
        set => SetAndRaise(ref _autoTail, value);
    }
        
    public int PageSize
    {
        get => _pageSize;
        set => SetAndRaise(ref _pageSize, value);
    }

    public int FirstIndex
    {
        get => _firstIndex;
        set => SetAndRaise(ref _firstIndex, value);
    }

    public bool ShowInline
    {
        get => _showInline;
        set => SetAndRaise(ref _showInline, value);
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