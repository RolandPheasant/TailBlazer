using System;
using System.Collections.ObjectModel;
using System.IO;
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

        public ReadOnlyObservableCollection<LineProxy> Lines => _data;

        public IProperty<int> SelectedItemsCount { get; }
        public IProperty<bool> ShouldHightlightMatchingText { get; }
        public IProperty<string> SearchHint { get; }
        public IProperty<int> MatchedLineCount { get; }
        public IProperty<string> FileSizeText { get; }
        public IProperty<bool> IsLoading { get; }
        public IProperty<string> HightlightText { get; }
        public ICommand CopyToClipboardCommand { get; }
        public ICommand KeepSearchCommand { get; }
        public ISelectionMonitor SelectionMonitor { get; }
        public SearchCollection SearchCollection { get; }


        public FileTailerViewModel([NotNull] ILogger logger,
            [NotNull] ISchedulerProvider schedulerProvider,
            [NotNull] FileInfo fileInfo,
            [NotNull] IFileTailerFactory fileTailerFactory, 
            [NotNull] ISelectionMonitor selectionMonitor, 
            [NotNull] IClipboardHandler clipboardHandler, 
            [NotNull] ISearchInfoCollection searchInfoCollection)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
            if (selectionMonitor == null) throw new ArgumentNullException(nameof(selectionMonitor));
            if (clipboardHandler == null) throw new ArgumentNullException(nameof(clipboardHandler));
            if (searchInfoCollection == null) throw new ArgumentNullException(nameof(searchInfoCollection));

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
            var tailer = fileTailerFactory.Create(fileInfo, SearchCollection.Latest.ObserveOn(schedulerProvider.Background), scroller);

            //command to add the current search to the tail collection
            KeepSearchCommand = new Command(() =>
            {
                var text = SearchText;
                var latest = fileInfo.Search(s => s.Contains(text, StringComparison.OrdinalIgnoreCase));
                searchInfoCollection.Add(text, latest);
                SearchText = string.Empty;
            },()=> SearchText.IsLongerThanOrEqualTo(3));


            //User feedback for when tailer is loading
            IsLoading = Observable.Timer(TimeSpan.FromSeconds(3)).Select(_ => true).Concat(Observable.Return(false)).ForBinding();  //tailer.IsLoading.ForBinding();

            var fileWatcher = new FileWatcher(fileInfo);

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
            var loader = tailer.Lines.Connect()
                .Transform(line => new LineProxy(line))
                .Sort(SortExpressionComparer<LineProxy>.Ascending(proxy => proxy))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _data)
                .Subscribe(changes => logger.Info($"Rows changed. {changes.Adds} adds, {changes.Removes} removed"), 
                            ex => logger.Error(ex, "There is a problem with bind data"));
            
            //monitor matching lines and start index,
            MatchedLineCount = SearchCollection.Latest.Select(latest=>latest.Count).ForBinding();

            //track first visible index
            var firstIndexMonitor = tailer.Lines.Connect()
                .Buffer(TimeSpan.FromMilliseconds(250)).FlattenBufferResult()
                .QueryWhenChanged(lines =>lines.Count == 0 ? 0 : lines.Select(l => l.Index).Min())
                .Subscribe(first=> FirstIndex= first);

            //add a search for all
            searchInfoCollection.Add("<All>", fileInfo.WatchFile()
                                                    .DistinctUntilChanged()
                                                    .TakeWhile(notification => notification.Exists).Repeat()
                                                    .Index(),true);

            _cleanUp = new CompositeDisposable(tailer,
                loader,
                firstIndexMonitor,
                IsLoading,
                MatchedLineCount,
                FileSizeText,
                SearchHint,
                ShouldHightlightMatchingText,


                SelectedItemsCount,
                searchInfoCollection,
                Disposable.Create(() =>
                {
                    _userScrollRequested.OnCompleted();
                    (SelectionMonitor as IDisposable)?.Dispose();
                }));
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
            {
                AutoTail = false;
            }
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
        
        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
