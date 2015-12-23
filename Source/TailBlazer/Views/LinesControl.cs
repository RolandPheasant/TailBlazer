using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TailBlazer.Views
{

    public class LinesControl : ListBox
    {

        static LinesControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LinesControl), new FrameworkPropertyMetadata(typeof(LinesControl)));
        }


        public static readonly DependencyProperty AnimationSetterProperty = DependencyProperty.RegisterAttached(
            "AnimationSetter", typeof (ListboxRowAnimationSetter), typeof (LinesControl), new PropertyMetadata(default(ListboxRowAnimationSetter), OnAnimationSetterChanged));

        public ListboxRowAnimationSetter AnimationSetter
        {
            get { return (ListboxRowAnimationSetter) GetValue(AnimationSetterProperty); }
            set { SetValue(AnimationSetterProperty, value); }
        }

        public static void OnAnimationSetterChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
           (sender as LinesControl).AddChild(args.NewValue);
        }   


        //public static readonly DependencyProperty HighlightDurationProperty = DependencyProperty.Register(
        //    "HighlightDuration", typeof (Duration), typeof (LinesControl), new PropertyMetadata(default(Duration)));



        //public Duration HighlightDuration
        //{
        //    get { return (Duration) GetValue(HighlightDurationProperty); }
        //    set { SetValue(HighlightDurationProperty, value); }
        //}

        //public static readonly DependencyProperty HightlightNewLinesProperty = DependencyProperty.Register(
        //    "HightlightNewLines", typeof (bool), typeof (LinesControl), new PropertyMetadata(default(bool)));

        //public bool HightlightNewLines
        //{
        //    get { return (bool) GetValue(HightlightNewLinesProperty); }
        //    set { SetValue(HightlightNewLinesProperty, value); }
        //}

        //public static readonly DependencyProperty HighliProperty = DependencyProperty.Register(
        //    "Highli", typeof (Brush), typeof (LinesControl), new PropertyMetadata(default(Brush)));

        //public Brush Highli
        //{
        //    get { return (Brush) GetValue(HighliProperty); }
        //    set { SetValue(HighliProperty, value); }
        //}
    }
}
