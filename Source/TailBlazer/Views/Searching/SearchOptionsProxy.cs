using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using DynamicData.Binding;
using DynamicData.Kernel;
using MaterialDesignThemes.Wpf;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views.Searching
{
    public class SearchOptionsProxy: AbstractNotifyPropertyChanged, IDisposable, IEquatable<SearchOptionsProxy>
    {
        private readonly IDisposable _cleanUp;
        private readonly SearchMetadata _searchMetadata;
        private readonly IDefaultIconSelector _defaultIconSelector;
        private bool _highlight;
        private bool _filter;
        private bool _useRegex;
        private bool _caseSensitive;
        private bool _isExclusion;
        private Hue _highlightHue;
        private PackIconKind _iconKind;
        private int _position;
      

        public bool IsGlobal { get; }
        public string Text => _searchMetadata.SearchText;
        public string RemoveTooltip => $"Get rid of {Text}?";
        public string ChangeScopeToolTip => IsGlobal ? "Change to local scope" : "Change to global scope";
        public ICommand RemoveCommand { get; }
        public ICommand HighlightCommand { get; }
        public Command ChangeScopeCommand { get; }
        public IEnumerable<Hue> Hues { get; }
        public ICommand ShowIconSelectorCommand { get; }
        private IconSelector IconSelector { get; }
        public Guid ParentId { get; }
        public IProperty<Brush> Background { get; }
        public IProperty<Brush> Foreground { get; }

        public SearchOptionsProxy([NotNull] SearchMetadata searchMetadata,
            [NotNull] Action<SearchMetadata> changeScopeAction,
            [NotNull] IColourProvider colourProvider, 
            [NotNull] IThemeProvider themeProvider,
            [NotNull] IconSelector iconSelector,
            [NotNull] Action<SearchMetadata> removeAction, 
            [NotNull] IDefaultIconSelector defaultIconSelector,
            Guid parentId)
        {
            if (searchMetadata == null) throw new ArgumentNullException(nameof(searchMetadata));
            if (changeScopeAction == null) throw new ArgumentNullException(nameof(changeScopeAction));
            if (colourProvider == null) throw new ArgumentNullException(nameof(colourProvider));
            if (themeProvider == null) throw new ArgumentNullException(nameof(themeProvider));
            if (iconSelector == null) throw new ArgumentNullException(nameof(iconSelector));
            if (removeAction == null) throw new ArgumentNullException(nameof(removeAction));
            if (defaultIconSelector == null) throw new ArgumentNullException(nameof(defaultIconSelector));

            _searchMetadata = searchMetadata;
            _defaultIconSelector = defaultIconSelector;

            IconSelector = iconSelector;
            ParentId = parentId;
            Highlight = _searchMetadata.Highlight;
            Filter = _searchMetadata.Filter;
            UseRegex = searchMetadata.UseRegex;
            CaseSensitive = !searchMetadata.IgnoreCase;
            Position = searchMetadata.Position;
            Hues = colourProvider.Hues;
            HighlightHue = searchMetadata.HighlightHue;
            IsGlobal = searchMetadata.IsGlobal;
            IsExclusion = searchMetadata.IsExclusion;

            ShowIconSelectorCommand = new Command(async () => await ShowIconSelector());
            RemoveCommand = new Command(() => removeAction(searchMetadata));
            ChangeScopeCommand = new Command(()=>changeScopeAction((SearchMetadata)this));
            HighlightCommand = new Command<Hue>(newHue =>
            {
                HighlightHue = newHue;
            });
            
            IconKind = _searchMetadata.IconKind.ParseEnum<PackIconKind>()
                            .ValueOr(() => PackIconKind.ArrowRightBold);

            //combine system with user choice.
            var defaultHue = this.WhenValueChanged(vm => vm.HighlightHue)
                    .CombineLatest(themeProvider.Accent, (user, system) => user == Hue.NotSpecified ? system : user)
                    .Publish();

            Foreground = defaultHue.Select(h => h.ForegroundBrush).ForBinding();
            Background = defaultHue.Select(h => h.BackgroundBrush).ForBinding();

            _cleanUp = new CompositeDisposable(IconSelector,Foreground,Background, defaultHue.Connect());
        }


        private async Task ShowIconSelector()
        {
            var dialogResult = await DialogHost.Show(IconSelector, ParentId);

            var result = (IconSelectorResult)dialogResult;
            if (result == IconSelectorResult.UseDefault)
            {
                //Use default
                var icon = _defaultIconSelector.GetIconFor(Text, UseRegex);
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
            private set { SetAndRaise(ref _useRegex, value); }
        }

        public bool CaseSensitive
        {
            get { return _caseSensitive; }
            set { SetAndRaise(ref _caseSensitive, value); }
        }

        public bool IsExclusion
        {
            get { return _isExclusion; }
            private set { SetAndRaise(ref _isExclusion, value); }
        }

        public int Position
        {
            get { return _position; }
            set { SetAndRaise(ref _position, value); }
        }

        public static explicit operator SearchMetadata(SearchOptionsProxy proxy)
        {
            return new SearchMetadata(proxy.Position, 
                proxy.Text, proxy.
                Filter,
                proxy.Highlight,
                proxy.UseRegex,
                !proxy.CaseSensitive,
                proxy.HighlightHue, 
                proxy.IconKind.ToString(),
                proxy.IsGlobal,
                proxy.IsExclusion);
        }

        #region Equality

        public bool Equals(SearchOptionsProxy other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_searchMetadata, other._searchMetadata);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SearchOptionsProxy) obj);
        }

        public override int GetHashCode()
        {
            return _searchMetadata?.GetHashCode() ?? 0;
        }

        public static bool operator ==(SearchOptionsProxy left, SearchOptionsProxy right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SearchOptionsProxy left, SearchOptionsProxy right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"SearchOptionsProxy: {_searchMetadata}";
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}