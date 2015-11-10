using System;
using System.Collections.ObjectModel;
using System.IO;
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
    public class FileTailerViewModel: AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly IDisposable _cleanUp;
        private readonly ReadOnlyObservableCollection<LineProxy> _data;
        private string _searchText;
        private bool _tailing;
        private string _lineCountText;

        public FileTailerViewModel(ILogger logger,ISchedulerProvider schedulerProvider, FileInfo fileInfo)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));

            File = fileInfo.FullName;
            Tailing = true;


            var tailer = new FileTailer(fileInfo, this.WhenValueChanged(vm=>vm.SearchText).Throttle(TimeSpan.FromMilliseconds(125)),Observable.Return(new ScrollRequest(40)));
            var lineCounter = tailer.TotalLines.CombineLatest(tailer.MatchedLines,(total,matched)=>
            {
                return total == matched.Length 
                    ? $"File has {total.ToString("#,###")} lines" 
                    : $"Showing {matched.Length.ToString("#,###")} of {total.ToString("#,###")} lines";
            })
            .Subscribe(text => LineCountText=text);


            var loader = tailer.Lines.Connect()
                .Buffer(TimeSpan.FromMilliseconds(125)).FlattenBufferResult()
                .Transform(line => new LineProxy(line))
                .Sort(SortExpressionComparer<LineProxy>.Ascending(proxy => proxy.Number))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _data)
                .Do(_=> AutoScroller.ScrollToEnd())
                .Subscribe(a => logger.Info(a.Adds.ToString()), ex => logger.Error(ex, "Oops"));



            _cleanUp = new CompositeDisposable(tailer, lineCounter, loader);

        }

        public string File { get; }

        public ReadOnlyObservableCollection<LineProxy> Lines => _data;
        
        public AutoScroller AutoScroller { get; } = new AutoScroller();

        public bool Tailing
        {
            get { return _tailing; }
            set { SetAndRaise(ref _tailing, value); }
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
