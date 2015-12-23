using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Settings;

namespace TailBlazer.Views
{
    public class SearchHints : AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly ReadOnlyObservableCollection<string> _hints;
        private readonly IDisposable _cleanUp;
        private string _searchText;

        public SearchHints(IRecentSearchCollection recentSearchCollection, ISchedulerProvider schedulerProvider)
        {
            //build a predicate when SearchText changes
            var filter = this.WhenValueChanged(t => t.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Select(BuildFilter);


            //observe customers and currency pairs using OR operator, and bind to the observable collection
            _cleanUp = recentSearchCollection.Items.Connect()
                .Filter(filter)     //filter strings
                .Transform(recentSearch=> recentSearch.Text)
                .Sort(SortExpressionComparer<string>.Ascending(str => str))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out _hints)       //bind to hints list
                .Subscribe();
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

        public ReadOnlyObservableCollection<string> Hints => _hints;

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}