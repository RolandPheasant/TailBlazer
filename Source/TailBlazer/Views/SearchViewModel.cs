using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using DynamicData.Binding;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views
{
    public class SearchViewModel : AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly SearchInfo _tail;
        private readonly IDisposable _cleanUp;
        private int _count;
        private bool _searching;
        private int _segments;
        private int _segmentsSearched;
        private string _countText;

        public ICommand  RemoveCommand { get; }

        public string Text => _tail.SearchText.ToUpper();

        public string RemoveTooltip => $"Get rid of {Text}?";

        public bool IsUserDefined => _tail.SearchType == SearchType.User ;

        public SearchType SearchType => _tail.SearchType ;

        public IObservable<ILineProvider> Latest => _tail.Latest;

        public SearchViewModel(SearchInfo tail, Action<SearchViewModel> removeAction)
        {
            _tail = tail;
            RemoveCommand = new Command(()=> removeAction(this));
            var counter = _tail.Latest
                .Select(lp => lp.Count)
                .Subscribe(count => Count = count);

            var counterTextFormatter = _tail.Latest
                .Select(lp =>
                {
                    var limited = lp as IHasLimitationOfLines;
                    if (limited == null) return $"{lp.Count.ToString("#,###0")}";
                    return limited.HasReachedLimit 
                                ? $"{limited.Maximum.ToString("#,###0")}+" 
                                : $"{lp.Count.ToString("#,###0")}";
                })
                 .Subscribe(countText => CountText = countText); ;


            var progressMonitor = _tail.Latest.OfType<IProgressInfo>().Subscribe(result =>
            {
                Searching = result.IsSearching;
                Segments = result.Segments;
                SegmentsSearched = result.SegmentsCompleted;
            });

            _cleanUp = new CompositeDisposable(progressMonitor, counter, counterTextFormatter);
        }


        public int Count
        {
            get { return _count; }
            set { SetAndRaise(ref _count, value); }
        }

        public string CountText
        {
            get { return _countText; }
            set { SetAndRaise(ref _countText, value); }
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