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

namespace TailBlazer.Views
{
    public class FileTailerViewModel: AbstractNotifyPropertyChanged, IDisposable, IScrollReceiver
    {
        private readonly IDisposable _cleanUp;
        private readonly ReadOnlyObservableCollection<LineProxy> _data;
        private readonly ISubject<ScrollRequest> _userScrollRequested = new ReplaySubject<ScrollRequest>(1);
        private string _searchText;
        private bool _autoTail=true;
        private string _lineCountText;
        private int _firstIndex;
        private int _matchedLineCount;
        private int _pageSize;
        private string _fileSizeText;
        private string _searchHint;
        private bool _highlightMatchingText
            ;

        public ReadOnlyObservableCollection<LineProxy> Lines => _data;
        
        public FileTailerViewModel(ILogger logger,ISchedulerProvider schedulerProvider, FileInfo fileInfo)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            var filterRequest = this.WhenValueChanged(vm => vm.SearchText).Throttle(TimeSpan.FromMilliseconds(125));
            var autoChanged = this.WhenValueChanged(vm => vm.AutoTail);
            var scroller = _userScrollRequested
                        .CombineLatest(autoChanged, (user, auto) =>
                        {
                            var mode = AutoTail ? ScrollingMode.Tail : ScrollingMode.User;
                            return  new ScrollRequest(mode, user.PageSize, user.FirstIndex);
                        })
                        .ObserveOn(schedulerProvider.TaskPool)
                        .DistinctUntilChanged();

            var tailer = new FileTailer(fileInfo, filterRequest, scroller);


            //create user display for count line count
            var lineCounter = tailer.TotalLines.CombineLatest(tailer.MatchedLines,(total,matched)=>
            {
                return total == matched 
                    ? $"{total.ToString("#,###")} lines" 
                    : $"{matched.ToString("#,###0")} of {total.ToString("#,###")} lines";
            })
            .Subscribe(text => LineCountText=text);

            var hintWriter = this.WhenValueChanged(vm => vm.SearchText)
                .Select(text =>
                {
                    if (string.IsNullOrEmpty(text))
                        return "Type to search";
                    return text.Length < 3 ? "Enter at least 3 characters" : "Filter applied";
                }).Subscribe(text=> SearchHint = text);

            var highlighter = this.WhenValueChanged(vm => vm.SearchText)
                .Subscribe(searchText => HighlightMatchingText = !string.IsNullOrEmpty(searchText) && searchText.Length >= 3);

            var sizeMonitor = tailer.FileSize
                .Select(size=> size.FormatWithAbbreviation())
                .DistinctUntilChanged()
                .Subscribe(text => FileSizeText = text);

            //load lines into observable collection
            var loader = tailer.Lines.Connect()
                .Transform(line => new LineProxy(line))
                .Sort(SortExpressionComparer<LineProxy>.Ascending(proxy => proxy.Number))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _data)
                .Subscribe(changes => logger.Info($"Rows changed {changes.Adds} adds, {changes.Removes} removed"), 
                            ex => logger.Error(ex, "There is a problem with bind data"));


            //monitor matching lines and start index,
            var matchedLinesMonitor = tailer.MatchedLines
                .Subscribe(matched => MatchedLineCount = matched);

            //track first visible index
            var firstIndexMonitor = tailer.Lines.Connect()
                .QueryWhenChanged(lines =>lines.Count == 0 ? 0 : lines.Select(l => l.Index).Min())
                .Subscribe(first=> FirstIndex= first);
            

            _cleanUp = new CompositeDisposable(tailer, 
                lineCounter, 
                loader,
                firstIndexMonitor,
                matchedLinesMonitor,
                sizeMonitor,
                hintWriter,
                highlighter,
                Disposable.Create(() =>
                {
                    _userScrollRequested.OnCompleted();
                }));
        }
        void IScrollReceiver.ScrollBoundsChanged(ScrollBoundsArgs boundsArgs)
        {
            if (boundsArgs == null) throw new ArgumentNullException(nameof(boundsArgs));
            var mode = AutoTail ? ScrollingMode.Tail : ScrollingMode.User;

            /*
                I need to get rid of this subject as I prefer functional over imperative. 
                However due to complexities between the interactions with the VirtualScrollPanel
                each time I have tried to remove it all hell has broken loose
            */
            _userScrollRequested.OnNext(new ScrollRequest(mode, boundsArgs.PageSize,boundsArgs.FirstIndex));
            PageSize = boundsArgs.PageSize;
            FirstIndex = boundsArgs.FirstIndex;
        }

        void IScrollReceiver.ScrollChanged(ScrollChangedArgs scrollChangedArgs)
        {
            if (scrollChangedArgs.Direction== ScrollDirection.Up)
                AutoTail = false;
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

        public int MatchedLineCount
        {
            get { return _matchedLineCount; }
            set { SetAndRaise(ref _matchedLineCount, value); }
        }

        public bool HighlightMatchingText
        {
            get { return _highlightMatchingText; }
            set { SetAndRaise(ref _highlightMatchingText, value); }
        }


        public string SearchText
        {
            get { return _searchText; }
            set { SetAndRaise(ref _searchText, value); }
        }


        public string SearchHint
        {
            get { return _searchHint; }
            set { SetAndRaise(ref _searchHint, value); }
        }

        public string LineCountText
        {
            get { return _lineCountText; }
            set { SetAndRaise(ref _lineCountText, value); }
        }

        public string FileSizeText
        {
            get { return _fileSizeText; }
            set { SetAndRaise(ref _fileSizeText, value); }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
