using System;
using System.Collections.Generic;
using System.Windows.Input;
using DynamicData.Binding;
using MaterialDesignColors;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Infrastucture;

namespace TailBlazer.Settings
{
    public class SearchOptionsProxy: AbstractNotifyPropertyChanged
    {

        private readonly SearchMetadata _searchMetadata;
        private bool _highlight;
        private bool _filter;

        public string Text => _searchMetadata.SearchText.ToUpper();

        public string RemoveTooltip => $"Get rid of {Text}?";

        public ICommand RemoveCommand { get; }

        public IEnumerable<Swatch> Swatches { get; }

        public SearchOptionsProxy(SearchMetadata searchMetadata, IEnumerable<Swatch> swatches, Action<SearchMetadata> removeAction)
        {
            Swatches = swatches;
            if (searchMetadata == null) throw new ArgumentNullException(nameof(searchMetadata));
            if (swatches == null) throw new ArgumentNullException(nameof(swatches));
            if (removeAction == null) throw new ArgumentNullException(nameof(removeAction));

            _searchMetadata = searchMetadata;
            Highlight = _searchMetadata.Highlight;
            Filter = _searchMetadata.Filter;
            RemoveCommand = new Command(() => removeAction(searchMetadata));
        }

        public bool Highlight
        {
            get { return _highlight; }
            set { SetAndRaise(ref _highlight, value); }
        }

        public bool Filter
        {
            get { return _filter; }
            set { SetAndRaise(ref _filter, value); }
        }
    }
}