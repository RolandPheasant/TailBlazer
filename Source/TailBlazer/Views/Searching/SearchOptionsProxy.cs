using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;
using System.Windows.Media;
using DynamicData.Binding;
using DynamicData.Kernel;
using MaterialDesignThemes.Wpf;
using TailBlazer.Controls;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;
using Hue = TailBlazer.Domain.Formatting.Hue;

namespace TailBlazer.Views.Searching
{
    public class SearchOptionsProxy: AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly IDisposable _cleanUp;
        private readonly SearchMetadata _searchMetadata;
        private readonly IKnownIconNames _knownIconNames;
        private bool _highlight;
        private bool _filter;
        private bool _useRegex;
        private bool _ignoreCase;
        private Hue _highlightHue;
        private PackIconKind _iconKind;

        public string Text => _searchMetadata.SearchText;
        public string RemoveTooltip => $"Get rid of {Text}?";
        public ICommand RemoveCommand { get; }
        public ICommand HighlightCommand { get; }
        public IEnumerable<Hue> Hues { get; }

        public ICommand ShowIconSelectorCommand { get; }

        private IconSelector IconSelector { get; }

        public Guid ParentId { get; }
        public int Position => _searchMetadata.Position;
        public IProperty<Brush> Background { get; }
        public IProperty<Brush> Foreground { get; }

        public SearchOptionsProxy([NotNull] SearchMetadata searchMetadata, 
            [NotNull] IAccentColourProvider accentColourProvider, 
            [NotNull] IconSelector iconSelector,
            [NotNull] Action<SearchMetadata> removeAction, 
            [NotNull] IKnownIconNames knownIconNames,
            Guid parentId)
        {
            if (searchMetadata == null) throw new ArgumentNullException(nameof(searchMetadata));
            if (accentColourProvider == null) throw new ArgumentNullException(nameof(accentColourProvider));
            if (iconSelector == null) throw new ArgumentNullException(nameof(iconSelector));
            if (removeAction == null) throw new ArgumentNullException(nameof(removeAction));
            if (knownIconNames == null) throw new ArgumentNullException(nameof(knownIconNames));

            _searchMetadata = searchMetadata;
            _knownIconNames = knownIconNames;
            IconSelector = iconSelector;

            ShowIconSelectorCommand = new Command(ShowIconSelector);
            RemoveCommand = new Command(() => removeAction(searchMetadata));
            HighlightCommand = new Command<Hue>(newHue =>
            {
                HighlightHue = newHue;
            });
            
            ParentId = parentId;
            Highlight = _searchMetadata.Highlight;
            Filter = _searchMetadata.Filter;
            UseRegex = searchMetadata.UseRegex;
            IgnoreCase = searchMetadata.IgnoreCase;
            Hues = accentColourProvider.Hues;
            HighlightHue = searchMetadata.HighlightHue;

            IconKind = _searchMetadata.IconKind.ParseEnum<PackIconKind>()
                            .ValueOr(() =>
                            {
                                return knownIconNames.Selected
                                    .ParseEnum<PackIconKind>()
                                    .ValueOr(() => PackIconKind.ArrowRightBold);
                            });

            Foreground = this.WhenValueChanged(vm => vm.HighlightHue)
                .Select(h => h.ForegroundBrush)
                .ForBinding();

            Background = this.WhenValueChanged(vm => vm.HighlightHue)
               .Select(h => h.BackgroundBrush)
               .ForBinding();

            _cleanUp = new CompositeDisposable(IconSelector,  Foreground, Background);
        }
        
        private async void ShowIconSelector()
        {
            var dialogResult = await DialogHost.Show(IconSelector, ParentId);
            var result = (IconSelectorResult)dialogResult;
            if (result == IconSelectorResult.UseDefault)
            {
                //Use default
                var icon = _knownIconNames.SelectIconFor(Text, UseRegex);
                IconKind = icon.ParseEnum<PackIconKind>().ValueOr(() => PackIconKind.ArrowRightBold);
            }
            else if (result == IconSelectorResult.UseSelected)
            {
                IconKind = IconSelector.Selected.Type;
            }
         }
        
        public PackIconKind IconKind
        {
            get { return _iconKind; }
            set { SetAndRaise(ref _iconKind, value); }
        }

        public bool Highlight
        {
            get { return _highlight; }
            set { SetAndRaise(ref _highlight, value); }
        }

        private Hue HighlightHue
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
            private set { SetAndRaise(ref _useRegex, value); }
        }

        public bool IgnoreCase
        {
            get { return _ignoreCase; }
            set { SetAndRaise(ref _ignoreCase, value); }
        }

        public static explicit operator SearchMetadata(SearchOptionsProxy proxy)
        {
            return new SearchMetadata(proxy.Position, 
                proxy.Text, proxy.
                Filter,
                proxy.Highlight,
                proxy.UseRegex,
                proxy.IgnoreCase,
                proxy.HighlightHue, 
                proxy.IconKind.ToString());
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}