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
using TailBlazer.Infrastructure;
using TailBlazer.Views.Recent;

namespace TailBlazer.Views.Searching;

public class SearchHints : AbstractNotifyPropertyChanged, IDisposable, IDataErrorInfo
{
    private readonly IDisposable _cleanUp;
    private readonly ReadOnlyObservableCollection<string> _hints;

    private readonly RegexInspector _regexInspector = new RegexInspector();
    private string _searchText;
    private bool _useRegex;


    public SearchHints(IRecentSearchCollection recentSearchCollection, ISchedulerProvider schedulerProvider)
    {
        //User feedback to guide them whilst typing
        var searchText = this.WhenValueChanged(vm => vm.SearchText);
        var useRegEx = this.WhenValueChanged(vm => vm.UseRegex);

        //if regex then validate

        var combined = searchText.CombineLatest(useRegEx, (text, regex) => new SearchRequest(text, regex))
            .Throttle(TimeSpan.FromMilliseconds(250))
            .Select(searchRequest => searchRequest.BuildMessage())
            .Publish();

        IsValid = combined.Select(shm => shm.IsValid).DistinctUntilChanged().ForBinding();
        Message = combined.Select(shm => shm.Message).DistinctUntilChanged().ForBinding();

        var forceRefreshOfError = combined.Select(shm => shm.IsValid)
            .DistinctUntilChanged()
            .Subscribe(_ => { OnPropertyChanged(nameof(SearchText)); });

        var predictRegex = this.WhenValueChanged(vm => vm.SearchText)
            .Select(text => _regexInspector.DoesThisLookLikeRegEx(text))
            .Subscribe(likeRegex => UseRegex = likeRegex);

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
        }, () => IsValid.Value && SearchText.Length > 0);


        var dataLoader = recentSearchCollection.Items.Connect()
            // .Filter(filter)
            .Transform(recentSearch => recentSearch.Text)
            .Sort(SortExpressionComparer<string>.Ascending(str => str))
            .ObserveOn(schedulerProvider.MainThread)
            .Bind(out _hints)
            .Subscribe();

        _cleanUp = new CompositeDisposable(IsValid, Message, predictRegex, dataLoader, searchRequested.SetAsComplete(), combined.Connect(), forceRefreshOfError);
    }

    public ReadOnlyObservableCollection<string> Hints => _hints;

    public ICommand AddSearchCommand { get; }

    public IObservable<SearchRequest> SearchRequested { get; }

    public IProperty<bool> IsValid { get; }

    public IProperty<string> Message { get; }

    public string SearchText
    {
        get => _searchText;
        set => SetAndRaise(ref _searchText, value);
    }

    public bool UseRegex
    {
        get => _useRegex;
        set => SetAndRaise(ref _useRegex, value);
    }


    public void Dispose()
    {
        _cleanUp.Dispose();
    }

    private Func<RecentSearch, bool> BuildFilter(string searchText)
    {
        if (string.IsNullOrEmpty(searchText)) return trade => true;

        return recentSearch => recentSearch.Text.StartsWith(searchText, StringComparison.OrdinalIgnoreCase);
    }

    #region Data error 

    string IDataErrorInfo.this[string columnName] => IsValid.Value ? null : Message.Value;

    string IDataErrorInfo.Error => null;

    #endregion
}