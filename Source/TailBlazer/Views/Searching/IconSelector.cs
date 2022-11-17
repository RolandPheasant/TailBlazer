using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Views.Formatting;

namespace TailBlazer.Views.Searching;

public enum IconSelectorResult
{
    UseSelected,
    UseDefault,
    None
}

public class IconSelector : AbstractNotifyPropertyChanged, IDisposable
{
    private readonly IDisposable _cleanUp;
    private string _iconSearchText;
    private IconDescription _selected;
    public IProperty<bool> HasSelection { get; }

    public ReadOnlyObservableCollection<IconDescription> Icons { get; }

    public IconSelector([NotNull] IIconProvider iconsProvider, [NotNull] ISchedulerProvider schedulerProvider)
    {
        if (iconsProvider == null) throw new ArgumentNullException(nameof(iconsProvider));
        if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            
        //build a predicate when SearchText changes
        var filter = this.WhenValueChanged(t => t.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .Select(BuildFilter);

        var userOptions = iconsProvider.Icons.Connect()
            .Filter(filter)
            .Sort(SortExpressionComparer<IconDescription>.Ascending(icon => icon.Name))
            .ObserveOn(schedulerProvider.MainThread)
            .Bind(out var icons)
            .Subscribe();

        HasSelection= this.WhenValueChanged(vm => vm.Selected)
            .Select(selected => selected != null)
            .ForBinding();

        Icons = icons;

        _cleanUp = new CompositeDisposable(userOptions);
    }
        
    public string SearchText
    {
        get => _iconSearchText;
        set => SetAndRaise(ref _iconSearchText, value);
    }

    public  IconDescription Selected
    {
        get => _selected;
        set => SetAndRaise(ref _selected, value);
    }

    private Func<IconDescription, bool> BuildFilter(string searchText)
    {
        if (string.IsNullOrEmpty(searchText)) return icon => true;

        return icon => icon.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }
        
    public void Dispose()
    {
        _cleanUp.Dispose();
    }

}