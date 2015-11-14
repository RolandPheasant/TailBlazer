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

        public string File { get; }
        public ReadOnlyObservableCollection<LineProxy> Lines => _data;
        public AutoScroller AutoScroller { get; } = new AutoScroller();

        private string _searchText;
        private bool _autoTail;
        private string _lineCountText;
        private int _firstIndex;
        private int _matchedLineCount;
        private int _pageSize;

        public FileTailerViewModel(ILogger logger,ISchedulerProvider schedulerProvider, FileInfo fileInfo)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            File = fileInfo.FullName;
            AutoTail = true;
            
            var filterRequest = this.WhenValueChanged(vm => vm.SearchText).Throttle(TimeSpan.FromMilliseconds(125));
            var autoChanged = this.WhenValueChanged(vm => vm.AutoTail);
            var scroller = _userScrollRequested
                        .CombineLatest(autoChanged, (user, auto) =>
                        {
                            var mode = AutoTail ? ScrollingMode.Tail : ScrollingMode.User;
                            return  new ScrollRequest(mode, user.PageSize, user.FirstIndex);
                        })
                        .Sample(TimeSpan.FromMilliseconds(50))
                        .DistinctUntilChanged();

            var tailer = new FileTailer(fileInfo, filterRequest, scroller);


            //create user display for count line count
            var lineCounter = tailer.TotalLines.CombineLatest(tailer.MatchedLines,(total,matched)=>
            {
                return total == matched 
                    ? $"File has {total.ToString("#,###")} lines" 
                    : $"Showing {matched.ToString("#,###")} of {total.ToString("#,###")} lines";
            })
            .Subscribe(text => LineCountText=text);
            

            //load lines into observable collection
            var loader = tailer.Lines.Connect()

                .Transform(line => new LineProxy(line))
                .Sort(SortExpressionComparer<LineProxy>.Ascending(proxy => proxy.Number))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _data)
                .Subscribe(a => logger.Info(a.Adds.ToString()), ex => logger.Error(ex, "Oops"));


            //monitor matching lines and start index 
            //update local values so the virtual scroll panel can bind to them
            var matchedLinesMonitor = tailer.MatchedLines
                .Subscribe(matched => MatchedLineCount = matched);

            var firstIndexMonitor = tailer.Lines.Connect()
                .QueryWhenChanged(lines =>
                {
                    //use zero based index rather than line number
                    return lines.Count == 0 ? 0 : lines.Select(l => l.Number).Min() -1;
                }).Subscribe(first=> FirstIndex= first);


            _cleanUp = new CompositeDisposable(tailer, 
                lineCounter, 
                loader,
                firstIndexMonitor,
                matchedLinesMonitor,
                Disposable.Create(() =>
                {
                    _userScrollRequested.OnCompleted();
                }));

        }

        void IScrollReceiver.ScrollTo(ScrollValues values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));


            var mode = AutoTail ? ScrollingMode.Tail : ScrollingMode.User;
            _userScrollRequested.OnNext(new ScrollRequest(mode, values.PageSize,values.FirstIndex));

            PageSize = values.PageSize;
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

        public string SearchText
        {
            get { return _searchText; }
            set { SetAndRaise(ref _searchText, value); }
        }

        public string LineCountText
        {
            get { return _lineCountText; }
            set { SetAndRaise(ref _lineCountText, value); }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }


    }
}
