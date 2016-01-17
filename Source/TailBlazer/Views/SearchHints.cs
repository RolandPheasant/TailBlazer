using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;
using TailBlazer.Settings;

namespace TailBlazer.Views
{
    public class RegexInspector
    {
        private  readonly char[] _specialChars = @"\!@#$%^&*()[]+?".ToCharArray();

        private readonly Regex _isPlainText;

        public RegexInspector()
        {

            _isPlainText = new Regex("^[a-zA-Z0-9 ]*$");
        }

        public bool DoesThisLookLikeRegEx(string text)
        {
            return !_isPlainText.IsMatch(text);
        }
    }

    public class SearchRequest
    {
        public string Text { get;  }
        public bool UseRegEx { get;  }

        public SearchRequest(string text, bool useRegEx)
        {
            Text = text;
            UseRegEx = useRegEx;
        }
    }
    public class SearchHintMessage
    {

        public static readonly SearchHintMessage Valid = new SearchHintMessage(true, null);

        public bool IsValid { get; }
        public string Message { get; }

        public SearchHintMessage(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }

    }
    public static class SearchRequestEx
    {
        public static SearchHintMessage BuildMessage(this SearchRequest source)
        {
            if (string.IsNullOrEmpty(source.Text))
                return new SearchHintMessage(true, $"Type to search using {(source.UseRegEx ? "reg ex" : "plain text")}");

            if (string.IsNullOrEmpty(source.Text))
                return SearchHintMessage.Valid;

            if (source.UseRegEx && !source.Text.IsLongerThanOrEqualTo(2))
                return new SearchHintMessage(false, "Reg ex must be at least 2 characters");

            if (!source.UseRegEx && !source.Text.IsLongerThanOrEqualTo(3))
                return new SearchHintMessage(false, "Search text must be at least 3 characters");

            try
            {
                var test = new Regex(source.Text);
            }
            catch (ArgumentException)
            {
                return new SearchHintMessage(false, "Invalid regular expression");
            }


            var message = $"Hit enter to search using {(source.UseRegEx ? "reg ex" : "plain text")}";

            return new SearchHintMessage(true, message);
        }
    }

    public class SearchHints : AbstractNotifyPropertyChanged, IDisposable, IDataErrorInfo
    {
        private readonly ReadOnlyObservableCollection<string> _hints;
        private readonly IDisposable _cleanUp;
        private string _searchText;
        private bool _useRegex;

        private readonly RegexInspector _regexInspector = new RegexInspector();


        public IProperty<SearchHintMessage> IsValid { get; }

        public ICommand AddSearchCommand { get; }

        public IObservable<SearchRequest> SearchRequested { get; } 


        public SearchHints(IRecentSearchCollection recentSearchCollection, ISchedulerProvider schedulerProvider)
        {
            //build a predicate when SearchText changes
            var filter = this.WhenValueChanged(t => t.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Select(BuildFilter);

            //User feedback to guide them whilst typing
            var searchText = this.WhenValueChanged(vm => vm.SearchText);
            var useRegEx = this.WhenValueChanged(vm => vm.UseRegex);

            //if regex then validate

            var combined = searchText.CombineLatest(useRegEx, (text, regex) => new SearchRequest(text, regex))
                .Publish();



            IsValid = combined.Select(searchRequest => searchRequest.BuildMessage()).ForBinding();
            

            var predictRegex = this.WhenValueChanged(vm => vm.SearchText)
                                        .Where(text=>!string.IsNullOrEmpty(text))
                                        .Select(text=>_regexInspector.DoesThisLookLikeRegEx(text))
                                        .DistinctUntilChanged()
                                        .Subscribe(likeRegex=> UseRegex= likeRegex);

            //Handle adding new search
            var searchRequested = new Subject<SearchRequest>();
            SearchRequested = searchRequested.AsObservable();
            AddSearchCommand = new Command(() =>
            {
                recentSearchCollection.Add(new RecentSearch(SearchText));
                searchRequested.OnNext(new SearchRequest(SearchText, UseRegex));
                SearchText = string.Empty;
                UseRegex = false;

            }, () => IsValid.Value.IsValid);


            var dataLoader = recentSearchCollection.Items.Connect()
                .Filter(filter)  
                .Transform(recentSearch=> recentSearch.Text)
                .Sort(SortExpressionComparer<string>.Ascending(str => str))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _hints) 
                .Subscribe();

            _cleanUp = new CompositeDisposable( dataLoader, IsValid, predictRegex, Disposable.Create(searchRequested.OnCompleted), combined.Connect());
        }



        private Func<RecentSearch, bool> BuildFilter(string searchText)
        {
            if (string.IsNullOrEmpty(searchText)) return trade => true;

            return recentSearch => recentSearch.Text.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                   recentSearch.Text.Contains(searchText, StringComparison.OrdinalIgnoreCase);
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

        public ReadOnlyObservableCollection<string> Hints => _hints;

        public void Dispose()
        {
            _cleanUp.Dispose();
        }

        #region Data error 
        
        string IDataErrorInfo.this[string columnName] => IsValid.Value.IsValid ? null : IsValid.Value.Message;

        string IDataErrorInfo.Error => null;

        #endregion



    }
}