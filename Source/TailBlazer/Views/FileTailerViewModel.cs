using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using DynamicData.Binding;
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
        private bool _searching;
        private int _segments;
        private int _segmentsSearched;

        public ReadOnlyObservableCollection<LineProxy> Lines => _data;

        public IProperty<bool> ShouldHightlightMatchingText { get; }
        public IProperty<string> LineCountText { get; }
        public IProperty<string> SearchHint { get; }
        public IProperty<int> MatchedLineCount { get; }
        public IProperty<string> FileSizeText { get; }
        public IProperty<bool> IsLoading { get; }

        public SelectionController<LineProxy> SelectionController { get; }= new SelectionController<LineProxy>();

        public FileTailerViewModel(ILogger logger,
            ISchedulerProvider schedulerProvider, 
            FileInfo fileInfo,
            IFileTailerFactory fileTailerFactory)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            //An observable which acts as a filter command
            var filterRequest = this.WhenValueChanged(vm => vm.SearchText).Throttle(TimeSpan.FromMilliseconds(125));

            //An observable which acts as a scroll command
            var autoChanged = this.WhenValueChanged(vm => vm.AutoTail);
            var scroller = _userScrollRequested.CombineLatest(autoChanged, (user, auto) =>
                        {
                            var mode = AutoTail ? ScrollReason.Tail : ScrollReason.User;
                            return  new ScrollRequest(mode, user.PageSize, user.FirstIndex);
                        })
                        .ObserveOn(schedulerProvider.Background)
                        .DistinctUntilChanged();

            var search = filterRequest.Select(searchText =>
            {
                if (string.IsNullOrEmpty(searchText) || searchText.Length < 3)
                    return Observable.Return(FileSearchResult.None);

                return fileInfo.Search(s => s.Contains(searchText, StringComparison.OrdinalIgnoreCase));

            }).Switch()
            .Replay(1).RefCount();

            var progressMonitor = search.Subscribe(result =>
            {
                Searching = result.IsSearching;
                Segments = result.Segments;
                SegmentsSearched = result.SegmentsCompleted;
            });

            //tailer is the main object used to tail, scroll and filter in a file
            var tailer = fileTailerFactory.Create(fileInfo, search, scroller);
            
            //User feedback for when tailer is loading
            IsLoading = tailer.IsLoading.ForBinding();


            ////User feedback lines count and filter matches
            LineCountText = tailer.TotalLines.CombineLatest(search, (total, matched) =>
            {
                if (matched.IsEmpty) return $"{total.ToString("#,###")} lines";
            
                var limited = (IHasLimitationOfLines)matched ;
                return limited.HasReachedLimit 
                        ? $"{limited.Maximum.ToString("#,###0")}+ of {total.ToString("#,###")} lines" 
                        : $"{matched.Count.ToString("#,###0")} of {total.ToString("#,###")} lines";
            }).ForBinding();



            //User feedback to show file size
            FileSizeText = tailer.FileSize
                .Select(size => size.FormatWithAbbreviation())
                .DistinctUntilChanged()
                .ForBinding();

            //User feedback to guide them whilst typing
            SearchHint = this.WhenValueChanged(vm => vm.SearchText)
                            .Select(text =>
                            {
                                if (string.IsNullOrEmpty(text))
                                    return "Type to search";
                                return text.Length < 3 ? "Enter at least 3 characters" : "Filter applied";
                            }).ForBinding();

            //Only highlight search text when at least 3 letters have been entered
            ShouldHightlightMatchingText = this.WhenValueChanged(vm => vm.SearchText)
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
            MatchedLineCount = tailer.MatchedLines.ForBinding();

            //track first visible index
            var firstIndexMonitor = tailer.Lines.Connect()
                .Buffer(TimeSpan.FromMilliseconds(250)).FlattenBufferResult()
                .QueryWhenChanged(lines =>lines.Count == 0 ? 0 : lines.Select(l => l.Index).Min())
                .Subscribe(first=> FirstIndex= first);
           
            _cleanUp = new CompositeDisposable(tailer,
                LineCountText, 
                loader,
                firstIndexMonitor,
                IsLoading,
                MatchedLineCount,
                FileSizeText,
                SearchHint,
                ShouldHightlightMatchingText,
                progressMonitor,
                SelectionController,
                Disposable.Create(_userScrollRequested.OnCompleted));
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
        
        public bool Searching
        {
            get { return _searching; }
            set { SetAndRaise(ref _searching, value); }
        }

        public int Segments
        {
            get { return _segments; }
            set { SetAndRaise(ref _segments, value); }
        }

        public int SegmentsSearched
        {
            get { return _segmentsSearched; }
            set { SetAndRaise(ref _segmentsSearched, value); }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
