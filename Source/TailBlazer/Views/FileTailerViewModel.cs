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
        private readonly ISubject<ScrollValues> _userScrollRequested = new Subject<ScrollValues>();

        public string File { get; }
        public ReadOnlyObservableCollection<LineProxy> Lines => _data;
        public AutoScroller AutoScroller { get; } = new AutoScroller();

        private string _searchText;
        private bool _autoTail;
        private string _lineCountText;
        private int _firstRow;
        private int _matchedLineCount;

        public FileTailerViewModel(ILogger logger,ISchedulerProvider schedulerProvider, FileInfo fileInfo)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            File = fileInfo.FullName;
            AutoTail = true;
            
            var filterRequest = this.WhenValueChanged(vm => vm.SearchText).Throttle(TimeSpan.FromMilliseconds(125));
            var autotail = this.WhenValueChanged(vm => vm.AutoTail)
                            .CombineLatest(_userScrollRequested, (auto, user) =>
                            {
                                return auto ? new ScrollRequest(user.Rows) 
                                : new ScrollRequest(user.Rows, user.FirstIndex+1);
                            }).DistinctUntilChanged();

            var tailer = new FileTailer(fileInfo, filterRequest, autotail);


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
                .Buffer(TimeSpan.FromMilliseconds(125)).FlattenBufferResult()
                .Transform(line => new LineProxy(line))
                .Sort(SortExpressionComparer<LineProxy>.Ascending(proxy => proxy.Number))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _data)
                .Do(_=> AutoScroller.ScrollToEnd())
                .Subscribe(a => logger.Info(a.Adds.ToString()), ex => logger.Error(ex, "Oops"));


            //monitor matching lines and start index 
            //update local values so the virtual scroll panel can bind to them
            var matchedLinesMonitor = tailer.MatchedLines
                .Subscribe(matched => MatchedLineCount = matched);

            var firstIndexMonitor = tailer.Lines.Connect()
                .QueryWhenChanged(lines =>
                {
                    //use zero based index rather than line number
                    return lines.Count == 0 ? 0 : lines.Select(l => l.Number).Min() -1 ;
                }).Subscribe(first=> FirstRow= first-1);


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

        void IScrollReceiver.RequestChange(ScrollValues values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            _userScrollRequested.OnNext(values);
        }

        public bool AutoTail
        {
            get { return _autoTail; }
            set { SetAndRaise(ref _autoTail, value); }
        }

        public int FirstRow
        {
            get { return _firstRow; }
            set { SetAndRaise(ref _firstRow, value); }
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
