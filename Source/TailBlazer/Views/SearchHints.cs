using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Settings;

namespace TailBlazer.Views
{

    public class SearchHints : AbstractNotifyPropertyChanged, IDisposable, IDataErrorInfo
    {
        private readonly ReadOnlyObservableCollection<string> _hints;
        private readonly IDisposable _cleanUp;
        private string _searchText;
        private bool _useRegex;
        
        public IProperty<string> SearchHint { get; }

        private IProperty<ValidationResult> IsValid { get; }


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
           var combined = searchText.CombineLatest(useRegEx, (text, regex) => new {text, regex}).Publish();

            IsValid = combined.Select(x =>
            {
                if (!x.regex)
                    return ValidationResult.Valid;

                if (x.text==null)
                    return new ValidationResult(false,"Regular expression cannot be null");
                try
                {
                    var test = Regex.Match("", x.text);
                }
                catch (ArgumentException)
                {
                    return new ValidationResult(true, "Invalid regular expression");
                }

                return new ValidationResult(true, null);
            }).ForBinding();


            SearchHint = combined.Select(x=>
                            {
                                if (string.IsNullOrEmpty(x.text))
                                    return $"Type to search using {(x.regex ? "reg ex" : "plain text")}";

                                return x.text.Length < 3 ? "Enter at least 3 characters" : "Hit enter for more options";
                            }).ForBinding();

            //observe customers and currency pairs using OR operator, and bind to the observable collection
            var dataLoader = recentSearchCollection.Items.Connect()
                .Filter(filter)     //filter strings
                .Transform(recentSearch=> recentSearch.Text)
                .Sort(SortExpressionComparer<string>.Ascending(str => str))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _hints)       //bind to hints list
                .Subscribe();

            _cleanUp = new CompositeDisposable(SearchHint, dataLoader, IsValid, combined.Connect());
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

        private class ValidationResult
        {

            public static readonly ValidationResult Valid = new ValidationResult(true, null);
            
            public bool IsValid { get;  }
            public string Message { get;  }

            public ValidationResult(bool isValid, string message)
            {
                IsValid = isValid;
                Message = message;
            }

        }

    }
}