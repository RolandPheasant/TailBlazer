using System;
using System.Collections.Generic;
using System.Windows.Input;
using DynamicData.Binding;
using MaterialDesignColors;
using TailBlazer.Controls;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;
using TailBlazer.Infrastucture;
using Hue = TailBlazer.Domain.Formatting.Hue;

namespace TailBlazer.Views.Searching
{
    public class SearchOptionsProxy: AbstractNotifyPropertyChanged
    {

        private readonly SearchMetadata _searchMetadata;
        private bool _highlight;
        private bool _filter;
        private bool _useRegex;
        private bool _ignoreCase;
        private Hue _highlightHue;

        public string Text => _searchMetadata.SearchText;

        public string RemoveTooltip => $"Get rid of {Text}?";

        public ICommand RemoveCommand { get; }

        public ICommand HighlightCommand { get; }

        public IEnumerable<Hue> Hues { get; }
        
        public SearchResultIndicatorStatus Status { get; }

        public int Position => _searchMetadata.Position;

        public SearchOptionsProxy(SearchMetadata searchMetadata,IAccentColourProvider accentColourProvider, Action<SearchMetadata> removeAction)
        {

            if (searchMetadata == null) throw new ArgumentNullException(nameof(searchMetadata));
            if (accentColourProvider == null) throw new ArgumentNullException(nameof(accentColourProvider));
            if (removeAction == null) throw new ArgumentNullException(nameof(removeAction));

            _searchMetadata = searchMetadata;
            Highlight = _searchMetadata.Highlight;
            Filter = _searchMetadata.Filter;
            UseRegex = searchMetadata.UseRegex;
            IgnoreCase = searchMetadata.IgnoreCase;
            Hues = accentColourProvider.Hues;
            HighlightHue = searchMetadata.HighlightHue;
            Status = searchMetadata.UseRegex ? SearchResultIndicatorStatus.Regex : SearchResultIndicatorStatus.Text;

            RemoveCommand = new Command(() => removeAction(searchMetadata));
            HighlightCommand = new Command<Hue>(newHue =>
            {
                HighlightHue = newHue;
            });
        }
        
        public bool Highlight
        {
            get { return _highlight; }
            set { SetAndRaise(ref _highlight, value); }
        }

        public Hue HighlightHue
        {
            get { return _highlightHue; }
            set { SetAndRaise(ref _highlightHue, value); }
        }

        public bool Filter
        {
            get { return _filter; }
            set { SetAndRaise(ref _filter, value); }
        }
        
        public bool UseRegex
        {
            get { return _useRegex; }
            set { SetAndRaise(ref _useRegex, value); }
        }

        public bool IgnoreCase
        {
            get { return _ignoreCase; }
            set { SetAndRaise(ref _ignoreCase, value); }
        }



        public static explicit operator SearchMetadata(SearchOptionsProxy proxy)
        {
            return new SearchMetadata(proxy.Position, proxy.Text, proxy.Filter,proxy.Highlight,proxy.UseRegex,proxy.IgnoreCase,proxy.HighlightHue);
        }
    }
}