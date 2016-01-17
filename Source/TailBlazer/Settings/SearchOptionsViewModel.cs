using System;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;
using TailBlazer.Views;

namespace TailBlazer.Settings
{
    public class SearchOptionsViewModel: AbstractNotifyPropertyChanged, IDisposable
    {
        //create text to add new option - default to highlight without search
        private readonly IDisposable _cleanUp;
        private string _searchText;

        public ReadOnlyObservableCollection<SearchOptionsProxy> Data { get; }


        public SearchHints SearchHints { get;  }

        public SearchOptionsViewModel(ISearchMetadataCollection metadataCollection, 
            ISchedulerProvider schedulerProvider,
            SearchHints searchHints)
        {
            SearchHints = searchHints;
            //TODO: options for colour

            var swatches = new SwatchesProvider().Swatches;
            
            ReadOnlyObservableCollection<SearchOptionsProxy> data;

            var userOptions = metadataCollection.Metadata.Connect()
                .WhereReasonsAre(ChangeReason.Add, ChangeReason.Remove) //ignore updates because we update from here
                .Transform(meta => new SearchOptionsProxy(meta, swatches,m => metadataCollection.Remove(m.SearchText)))
                .SubscribeMany(so =>
                {
                    //when a value changes, write the original value back to the cache
                    return so.WhenAnyPropertyChanged()
                        .Subscribe(_ => metadataCollection.Add(new SearchMetadata(so.Text, so.Filter, so.Highlight,so.UseRegex,so.IgnoreCase)));
                })
                .Sort(SortExpressionComparer<SearchOptionsProxy>.Ascending(proxy=>proxy.Text))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out data)
                .Subscribe();

            Data = data;

            //command to add the current search to the tail collection
            var searchInvoker = SearchHints.SearchRequested.Subscribe(request =>
            {
                metadataCollection.Add(new SearchMetadata(request.Text, false, true, request.UseRegEx, true));
            });

            
            _cleanUp = new CompositeDisposable(searchInvoker, userOptions, searchInvoker);
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