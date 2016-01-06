using System;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;

namespace TailBlazer.Settings
{
    public class SearchOptionsViewModel: AbstractNotifyPropertyChanged, IDisposable
    {
        //create text to add new option - default to highlight without search
        private readonly IDisposable _cleanUp;
        private string _searchText;

        public ReadOnlyObservableCollection<SearchOptionsProxy> Data { get; }

        public ICommand AddSearchCommand { get; }

        public SearchOptionsViewModel(ISearchMetadataCollection metadataCollection, ISchedulerProvider schedulerProvider)
        {
            //options for colour
            //options for highlight
            //option to filter
            
            var swatches = new SwatchesProvider().Swatches;


            ReadOnlyObservableCollection<SearchOptionsProxy> data;
            var userOptions = metadataCollection.Metadata
                .Connect()
                .Transform(meta => new SearchOptionsProxy(meta, swatches,m => metadataCollection.Remove(m.SearchText)))
                .Sort(SortExpressionComparer<SearchOptionsProxy>.Ascending(proxy=>proxy.Text))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out data)
                .Subscribe();
            Data = data;

            AddSearchCommand = new Command(() =>
            {
                    metadataCollection.Add(new SearchMetadata(SearchText,false,true));
                    SearchText = string.Empty;

            }, () => SearchText.IsLongerThanOrEqualTo(3) && !metadataCollection.Metadata.Lookup((CaseInsensitiveString)SearchText).HasValue);


            var commandRefresher = this.WhenValueChanged(vm => vm.SearchText)
                                    .Subscribe(_ => ((Command) AddSearchCommand).Refresh());

            _cleanUp = new CompositeDisposable(commandRefresher, userOptions);
        }


        public string SearchText
        {
            get { return _searchText; }
            set { SetAndRaise(ref _searchText, value); }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}