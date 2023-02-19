using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using DynamicData.Binding;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Infrastructure;

namespace TailBlazer.Views.Options;

public sealed class GeneralOptionsViewModel : AbstractNotifyPropertyChanged, IDisposable
{

    private readonly IDisposable _cleanUp;

    private bool _highlightTail;
    private double _highlightDuration;
    private double _scale;
    private bool _useDarkTheme;
    private int _rating;
    private bool _openRecentOnStartup;
    private bool _showLineNumbers;

    public GeneralOptionsViewModel(ISetting<GeneralOptions> setting)
    {
        var reader = setting.Value.Subscribe(options =>
        {
            UseDarkTheme = options.Theme== Theme.Dark;
            HighlightTail = options.HighlightTail;
            HighlightDuration = options.HighlightDuration;
            Scale = options.Scale;
            Rating = options.Rating;
            OpenRecentOnStartup = options.OpenRecentOnStartup;
            ShowLineNumbers = options.ShowLineNumbers;
        });

        RequiresRestart = setting.Value.Select(options => options.Rating)
            .HasChanged()
            .ForBinding();

        RestartCommand = new Command(() =>
        {
            Process.Start($@"TailBlazer.exe");
            Application.Current.Shutdown();
        });

        var writter = this.WhenAnyPropertyChanged()
            .Subscribe(vm =>
            {
                setting.Write(new GeneralOptions(UseDarkTheme ? Theme.Dark : Theme.Light, HighlightTail, HighlightDuration, Scale, Rating, OpenRecentOnStartup, ShowLineNumbers));
            });
            
        HighlightDurationText = this.WhenValueChanged(vm=>vm.HighlightDuration)
            .DistinctUntilChanged()
            .Select(value => value.ToString("0.00 Seconds"))
            .ForBinding();

        ScaleText = this.WhenValueChanged(vm => vm.Scale)
            .DistinctUntilChanged()
            .Select(value => $"{value} %" )
            .ForBinding();
            
        ScaleRatio = this.WhenValueChanged(vm => vm.Scale)
            .DistinctUntilChanged()
            .Select(value => (decimal) value/(decimal) 100)
            .ForBinding();

        _cleanUp = new CompositeDisposable(reader, writter,  HighlightDurationText, ScaleText, ScaleRatio);
    }

    public ICommand RestartCommand { get; }

    public IProperty<bool> RequiresRestart { get;  }

    public IProperty<decimal> ScaleRatio { get;  }

    public IProperty<string> ScaleText { get;  }

    public IProperty<string> HighlightDurationText { get; }

    public bool UseDarkTheme
    {
        get { return _useDarkTheme; }
        set { SetAndRaise(ref _useDarkTheme, value); }
    }

    public bool HighlightTail
    {
        get { return _highlightTail; }
        set { SetAndRaise(ref _highlightTail, value); }
    }

    public double HighlightDuration
    {
        get { return _highlightDuration; }
        set { SetAndRaise(ref _highlightDuration, value); }
    }

    public int Rating
    {
        get { return _rating; }
        set { SetAndRaise(ref _rating, value); }
    }

    public double Scale
    {
        get { return _scale; }
        set { SetAndRaise(ref _scale, (int)value); }
    }

    public bool OpenRecentOnStartup
    {
        get { return _openRecentOnStartup; }
        set { SetAndRaise(ref _openRecentOnStartup, value); }
    }

    public bool ShowLineNumbers
    {
        get { return _showLineNumbers; }
        set { SetAndRaise(ref _showLineNumbers, value); }
    }

    void IDisposable.Dispose()
    {
        _cleanUp.Dispose();
    }
}