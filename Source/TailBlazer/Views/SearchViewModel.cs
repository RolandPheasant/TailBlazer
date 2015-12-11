using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using DynamicData.Binding;
using TailBlazer.Domain.FileHandling;
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

        public ICommand  RemoveCommand { get; }

        public string Text => _tail.SearchText.ToUpper();

        public bool IsDefault => _tail.IsDefault;

        public IObservable<ILineProvider> Latest => _tail.Latest;

        public SearchViewModel(SearchInfo tail, Action<SearchViewModel> removeAction)
        {
            _tail = tail;
            RemoveCommand = new Command(()=> removeAction(this));
            var counter = _tail.Latest
                .Select(lp => lp.Count)
                .Subscribe(count => Count = count);

            var progressMonitor = _tail.Latest.OfType<FileSearchResult>().Subscribe(result =>
            {
                Searching = result.IsSearching;
                Segments = result.Segments;
                SegmentsSearched = result.SegmentsCompleted;
            });

            _cleanUp = new CompositeDisposable(progressMonitor, counter);
        }


        public int Count
        {
            get { return _count; }
            set { SetAndRaise(ref _count, value); }
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