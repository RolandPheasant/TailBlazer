using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData.Binding;
using MaterialDesignThemes.Wpf;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Views.Options
{
    public sealed class GeneralOptionsViewModel : AbstractNotifyPropertyChanged, IDisposable
    {
        private bool _highlightTail;
        private double _highlightDuration;
        private int _scale;

        private readonly IDisposable _cleanUp;
        private bool _useDarkTheme;

        public GeneralOptionsViewModel(ISetting<GeneralOptions> setting, ISchedulerProvider schedulerProvider)
        {
            var reader = setting.Value.Subscribe(options =>
            {
                UseDarkTheme = options.Theme== Theme.Dark;
                HighlightTail = options.HighlightTail;
                HighlightDuration = options.HighlightDuration;
                Scale = options.Scale;
            });

            var writter = this.WhenAnyPropertyChanged()
                .Subscribe(vm =>
                {
                    setting.Write(new GeneralOptions(UseDarkTheme ? Theme.Dark : Theme.Light, HighlightTail, HighlightDuration, Scale));
                });



            HighlightDurationText = this.WhenValueChanged(vm=>vm.HighlightDuration)
                                        .Select(value => value.ToString("0.00 Seconds"))
                                        .ForBinding();

            ScaleText = this.WhenValueChanged(vm => vm.Scale)
                                        
                                        .Select(value => $"{value} %" )
                                        .ForBinding();


            ScaleRatio= this.WhenValueChanged(vm => vm.Scale)
                                        .Select(value =>(decimal)value / (decimal)100)
                                       // .Sample(TimeSpan.FromMilliseconds(250))
                                        .ForBinding();

            _cleanUp = new CompositeDisposable(reader, writter,  HighlightDurationText, ScaleText, ScaleRatio);
        }

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

        public int Scale
        {
            get { return _scale; }
            set { SetAndRaise(ref _scale, value); }
        }



        void IDisposable.Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}