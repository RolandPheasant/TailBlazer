using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private readonly IDisposable _cleanUp;
        private SearchViewModel _selected;
        private int _count;

        public ReadOnlyObservableCollection<SearchViewModel> Items => _items;
        public IObservable<string> SelectedText { get; }
        public IObservable<ILineProvider> Latest { get; }

        public SearchCollection(ISearchInfoCollection searchInfoCollection, ISchedulerProvider schedulerProvider)
        {
            var viewModels = searchInfoCollection.Searches.Connect()
                .Transform(tail => new SearchViewModel(tail, vm =>
                {
                    searchInfoCollection.Remove(vm.Text);
                }))
                .DisposeMany()
                .AsObservableCache();
            
            var shared = viewModels.Connect().Publish();

            var binderLoader = shared
                .Sort(SortExpressionComparer<SearchViewModel>.Ascending(tvm => tvm.Text))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _items)
                .Subscribe();
            
            var autoSelector = shared.WhereReasonsAre(ChangeReason.Add)
                .Flatten()
                .Select(change => change.Current)
                .Subscribe(latest => Selected = latest);
            
            var removed = shared.WhereReasonsAre(ChangeReason.Remove)
                .Subscribe(_ => Selected = viewModels.Items.First());

            var counter = shared.ToCollection()
                .Subscribe(count => Count = count.Count);

           SelectedText = this.WhenValueChanged(sc => sc.Selected)
                                .Where(x => x != null)
                                .Select(svm => svm.Text)
                                .Replay(1).RefCount();

           Latest = this.WhenValueChanged(sc => sc.Selected)
                .Where(x=>x!=null)
                .Select(svm => svm.Latest).Switch().Replay(1).RefCount();

            _cleanUp = new CompositeDisposable(viewModels, binderLoader, counter, removed, autoSelector, shared.Connect());
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
}