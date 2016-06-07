using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using DynamicData.Kernel;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Views.Searching
{
    public class SearchCollection: AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly ReadOnlyObservableCollection<SearchViewModel> _items;
        private readonly IDisposable _cleanUp;
        private SearchViewModel _selected;
        private int _count;
        private readonly IObservableCache<SearchViewModel, string> _viewModels;

        public ReadOnlyObservableCollection<SearchViewModel> Items => _items;
        public IObservable<string> SelectedText { get; }
        public IObservable<ILineProvider> Latest { get; }

        public SearchCollection(ISearchInfoCollection searchInfoCollection, ISchedulerProvider schedulerProvider)
        {
            _viewModels = searchInfoCollection.Searches.Connect()
                .Transform(tail => new SearchViewModel(tail, vm =>
                {
                    searchInfoCollection.Remove(vm.Text);
                }))
                .DisposeMany()
                .AsObservableCache();
            
            var shared = _viewModels.Connect();//.Publish();

            var binderLoader = shared
                .Sort(SortExpressionComparer<SearchViewModel>
                               .Ascending(tvm => tvm.SearchType== SearchType.All ? 1:2)
                               .ThenByAscending(tvm => tvm.Text))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _items)
                .Subscribe();
            
            var autoSelector = shared.WhereReasonsAre(ChangeReason.Add)
                .Flatten()
                .Select(change => change.Current)
                .Subscribe(latest => Selected = latest);


            var removed = shared.WhereReasonsAre(ChangeReason.Remove)
                .Subscribe(_ => Selected = _viewModels.Items.First());

            var counter = shared.ToCollection()
                .Subscribe(count => Count = count.Count);

            var nullDodger = this.WhenValueChanged(sc => sc.Selected)
                .Where(x => x == null)
                .Subscribe(x => Selected =_viewModels.Items.First());


           SelectedText = this.WhenValueChanged(sc => sc.Selected)
                                .Where(x => x != null)
                                .Select(svm => svm.Text)
                                .Replay(1).RefCount();

           Latest = this.WhenValueChanged(sc => sc.Selected)
                .Where(x=>x!=null)
                .Select(svm => svm.Latest)
                .Switch()
                .Replay(1).RefCount();
            
            _cleanUp = new CompositeDisposable(_viewModels, binderLoader, counter, removed, autoSelector, nullDodger);
        }

        public void Select(string item)
        {
            _viewModels.Lookup(item)
                .IfHasValue(selected => Selected = selected);
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