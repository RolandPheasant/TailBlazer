using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Views
{
    public class SearchCollection: AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly ReadOnlyObservableCollection<SearchViewModel> _items;

        public ReadOnlyObservableCollection<SearchViewModel> Items => _items;

        private readonly IDisposable _cleanUp;
        private SearchViewModel _selected;
        private int _count;

        public SearchCollection(ITailCollection tailCollection, ISchedulerProvider schedulerProvider)
        {

            var viewModels = tailCollection
                .Tails.Connect()
                .Transform(tail => new SearchViewModel(tail))
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
        private int _count;
        private readonly IDisposable _cleanUp;

        public string Text => _tail.SearchText;

        public SearchViewModel(Tailer tail)
        {
            _tail = tail;

            var counter = _tail.Latest
                            .Select(lp => lp.Count)
                            .Subscribe(count => Count = count);

            _cleanUp = counter;
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

}