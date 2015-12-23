using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views
{
    //public interface IRowAnimationSetter
    //{
    //}

    public class ListboxRowAnimationSetter : FrameworkElement, IAttachedListBox
    {
        private ListBox _listBox;

        public ListboxRowAnimationSetter()
        {
           
        }

        //protected override Freezable CreateInstanceCore()
        //{
        //    return new ListboxRowAnimationSetter();
        //}

        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(
            "Duration", typeof (Duration), typeof (ListboxRowAnimationSetter), new PropertyMetadata(default(Duration), OnPropertyChanged));


        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground", typeof (Brush), typeof (ListboxRowAnimationSetter), new PropertyMetadata(default(Brush), OnPropertyChanged));


        //public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
        //    "IsEnabled", typeof (bool), typeof (ListboxRowAnimationSetter), new PropertyMetadata(default(bool), OnPropertyChanged));

        //public bool IsEnabled
        //{
        //    get { return (bool) GetValue(IsEnabledProperty); }
        //    set { SetValue(IsEnabledProperty, value); }
        //}

        public Brush Foreground
        {
            get { return (Brush) GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public Duration Duration
        {
            get { return (Duration) GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public void Receive(ListBox target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            _listBox = target;
  
        }

        public static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
        }

    }
}
