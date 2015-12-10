using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views
{
    public class SearchCollection: AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly ReadOnlyObservableCollection<SearchViewModel> _items;
        private readonly IDisposable _cleanUp;
        private SearchViewModel _selected;
        private int _count;

        public ReadOnlyObservableCollection<SearchViewModel> Items => _items;
        public IObservable<string> SelectedText { get; }
        public IObservable<ILineProvider> Latest { get; }

        public SearchCollection(ITailCollection tailCollection, ISchedulerProvider schedulerProvider)
        {

            var viewModels = tailCollection
                .Tails.Connect()
                .Transform(tail => new SearchViewModel(tail, vm =>
                {
                    tailCollection.Remove(vm.Text);
                }))
                .DisposeMany()
                .AsObservableCache();

            var binderLoader = viewModels.Connect()
                .Sort(SortExpressionComparer<SearchViewModel>.Ascending(tvm => tvm.Text))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _items)
                .Subscribe();
            
            var autoSelector = viewModels.Connect()
                .WhereReasonsAre(ChangeReason.Add)
                .Flatten()
                .Select(change => change.Current)
                .Subscribe(latest => Selected = latest);

            var counter = viewModels.Connect()
                .ToCollection()
                .Subscribe(count => Count = count.Count);

           SelectedText = this.WhenValueChanged(sc => sc.Selected)
                                .Where(x => x != null)
                                .Select(svm => svm.Text).Replay(1).RefCount();

           Latest = this.WhenValueChanged(sc => sc.Selected)
                .Where(x=>x!=null)
                .Select(svm => svm.Latest).Switch().Replay(1).RefCount();

            _cleanUp = new CompositeDisposable(viewModels, binderLoader, counter, autoSelector);
        }



        public SearchViewModel Selected
        {
            get { return _selected; }
            set { SetAndRaise(ref _selected,value);}
        }
        public int Count
        {
            get { return _count; }
            set { SetAndRaise(ref _count, value); }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }


    public class SearchViewModel : AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly Tailer _tail;
        private readonly IDisposable _cleanUp;
        private int _count;
        private bool _searching;
        private int _segments;
        private int _segmentsSearched;

        public ICommand  RemoveCommand { get; }

        public string Text => _tail.SearchText;

        public IObservable<ILineProvider> Latest => _tail.Latest;

        public SearchViewModel(Tailer tail, Action<SearchViewModel> removeAction)
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