using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;
using System.Windows.Media;
using DynamicData;
using DynamicData.Binding;
using MaterialDesignThemes.Wpf;
using TailBlazer.Controls;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;
using TailBlazer.Views.Formatting;
using Hue = TailBlazer.Domain.Formatting.Hue;

namespace TailBlazer.Views.Searching
{
    public class SearchOptionsProxy: AbstractNotifyPropertyChanged, IDisposable
    {
     
        private readonly IDisposable _cleanUp;
        private readonly SearchMetadata _searchMetadata;
        private bool _highlight;
        private bool _filter;
        private bool _useRegex;
        private bool _ignoreCase;
        private Hue _highlightHue;
        private PackIconKind? _iconKind;

        public string Text => _searchMetadata.SearchText;
        public string RemoveTooltip => $"Get rid of {Text}?";
        public ICommand RemoveCommand { get; }
        public ICommand HighlightCommand { get; }
        public IEnumerable<Hue> Hues { get; }
        public SearchResultIndicatorStatus Status { get; }
        public ICommand ShowIconSelectorCommand { get; }
        public IconSelector IconSelector { get; }
        public Guid Id { get; }
        public IProperty<bool> UsingCustomIcon { get; }
        public int Position => _searchMetadata.Position;
        public IProperty<Brush> Background { get; }
        public IProperty<Brush> Foreground { get; }

        public SearchOptionsProxy([NotNull] SearchMetadata searchMetadata, 
            [NotNull] IAccentColourProvider accentColourProvider, 
            [NotNull] IconSelector iconSelector,
            [NotNull] Action<SearchMetadata> removeAction, 
            Guid id)
        {
            IconSelector = iconSelector;

            if (searchMetadata == null) throw new ArgumentNullException(nameof(searchMetadata));
            if (accentColourProvider == null) throw new ArgumentNullException(nameof(accentColourProvider));
            if (iconSelector == null) throw new ArgumentNullException(nameof(iconSelector));
            if (removeAction == null) throw new ArgumentNullException(nameof(removeAction));
            _searchMetadata = searchMetadata;

            ShowIconSelectorCommand = new Command(ShowIconSelector);
            RemoveCommand = new Command(() => removeAction(searchMetadata));
            HighlightCommand = new Command<Hue>(newHue =>
            {
                HighlightHue = newHue;
            });
            
            Id = id;
            Highlight = _searchMetadata.Highlight;
            Filter = _searchMetadata.Filter;
            UseRegex = searchMetadata.UseRegex;
            IgnoreCase = searchMetadata.IgnoreCase;
            Hues = accentColourProvider.Hues;
            HighlightHue = searchMetadata.HighlightHue;
            Status = searchMetadata.UseRegex ? SearchResultIndicatorStatus.Regex : SearchResultIndicatorStatus.Text;

            UsingCustomIcon = this.WhenValueChanged(proxy => proxy.IconKind)
                .Select(kind => kind.HasValue)
                .ForBinding();

            Foreground = this.WhenValueChanged(vm => vm.HighlightHue)
                .Select(h => h.ForegroundBrush)
                .ForBinding();

            Background = this.WhenValueChanged(vm => vm.HighlightHue)
               .Select(h => h.BackgroundBrush)
               .ForBinding();

            _cleanUp = new CompositeDisposable(IconSelector, UsingCustomIcon, Foreground, Background);
        }



        private async void ShowIconSelector()
        {
            var result = await DialogHost.Show(IconSelector, Id);
            var accept = (bool?) result;
            if (accept.HasValue && accept == true)
            {
                IconKind = IconSelector.Selected.Type;
            }
            else
            {
                IconKind = null;
            }
         }
        
        public PackIconKind? IconKind
        {
            get { return _iconKind; }
            set { SetAndRaise(ref _iconKind, value); }
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

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}