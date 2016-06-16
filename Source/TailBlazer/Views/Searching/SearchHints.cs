using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;
using TailBlazer.Views.Recent;

namespace TailBlazer.Views.Searching
{
    public class SearchHints : AbstractNotifyPropertyChanged, IDisposable, IDataErrorInfo
    {
        private readonly ReadOnlyObservableCollection<string> _hints;
        private readonly IDisposable _cleanUp;
        private string _searchText;
        private bool _useRegex;

        private readonly RegexInspector _regexInspector = new RegexInspector();

        public ReadOnlyObservableCollection<string> Hints => _hints;

        public ICommand AddSearchCommand { get; }

        public IObservable<SearchRequest> SearchRequested { get; }

        public IProperty<bool> IsValid { get; }

        public IProperty<string> Message { get; }


        public SearchHints(IRecentSearchCollection recentSearchCollection, ISchedulerProvider schedulerProvider)
        {
            //User feedback to guide them whilst typing
            var searchText = this.WhenValueChanged(vm => vm.SearchText);
            var useRegEx = this.WhenValueChanged(vm => vm.UseRegex);

            //if regex then validate

            var combined = searchText.CombineLatest(useRegEx, (text, regex) => new SearchRequest(text, regex))
                .Select(searchRequest => searchRequest.BuildMessage())
                .Publish();

            IsValid = combined.Select(shm => shm.IsValid).ForBinding();
            Message = combined.Select(shm => shm.Message).ForBinding();

            var predictRegex = this.WhenValueChanged(vm => vm.SearchText)
                                        .Select(text=>_regexInspector.DoesThisLookLikeRegEx(text))
                                        .Subscribe(likeRegex=> UseRegex= likeRegex);

            //Handle adding new search
            var searchRequested = new Subject<SearchRequest>();
            SearchRequested = searchRequested.AsObservable();
            AddSearchCommand = new Command(async () =>
            {
                await Task.Run(() =>
                {
                    recentSearchCollection.Add(new RecentSearch(SearchText));
                    searchRequested.OnNext(new SearchRequest(SearchText, UseRegex));
                    SearchText = string.Empty;
                    UseRegex = false;
                });

            }, () => IsValid.Value && SearchText.Length>0);


            var dataLoader = recentSearchCollection.Items.Connect()
               // .Filter(filter)
                .Transform(recentSearch => recentSearch.Text)
                .Sort(SortExpressionComparer<string>.Ascending(str => str))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _hints)
                .Subscribe();

            _cleanUp = new CompositeDisposable( IsValid,Message, predictRegex, dataLoader, searchRequested.SetAsComplete(), combined.Connect());
        }
        
        private Func<RecentSearch, bool> BuildFilter(string searchText)
        {
            if (string.IsNullOrEmpty(searchText)) return trade => true;

            return recentSearch => recentSearch.Text.StartsWith(searchText, StringComparison.OrdinalIgnoreCase);
        }
        
        public string SearchText
        {
            get { return _searchText; }
            set { SetAndRaise(ref _searchText, value); }
        }

        public bool UseRegex
        {
            get { return _useRegex; }
            set { SetAndRaise(ref _useRegex, value); }
        }

        #region Data error 
        
        string IDataErrorInfo.this[string columnName] => IsValid.Value ? null : Message.Value;

        string IDataErrorInfo.Error => null;

        #endregion


        public void Dispose()
        {
            _cleanUp.Dispose();
        }

    }
}