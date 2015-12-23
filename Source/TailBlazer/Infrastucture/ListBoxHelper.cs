using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TailBlazer.Views;

namespace TailBlazer.Infrastucture
{
    public  class ListBoxHelper: FrameworkElement
    {



        public static readonly DependencyProperty SelectionMonitorProperty = DependencyProperty.RegisterAttached("SelectionMonitor", typeof(IAttachedListBox), typeof(ListBoxHelper),
            new PropertyMetadata(default(ISelectionMonitor), PropertyChanged));

        public static void SetSelectionMonitor(Selector element, ISelectionMonitor value)
        {
            element.SetValue(SelectionMonitorProperty, value);
        }

        public static ISelectionMonitor GetSelectionMonitor(Selector element)
        {
            return (ISelectionMonitor)element.GetValue(SelectionMonitorProperty);
        }

        public static void PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            //var receiver = args.NewValue as IAttachedListBox;
         //   receiver?.Receive((ListBox)sender);
        }



        public static readonly DependencyProperty RowAnimationSetterProperty = DependencyProperty.RegisterAttached(
            "RowAnimationSetter", typeof (ListboxRowAnimationSetter), typeof (ListBoxHelper),
            new PropertyMetadata(default(ListboxRowAnimationSetter), PropertyChanged));


        public static void SetRowAnimationSetter(Selector element, ListboxRowAnimationSetter value)
        {
            element.SetValue(RowAnimationSetterProperty, value);
        }

        public static ListboxRowAnimationSetter GetRowAnimationSetter(Selector element)
        {
            return (ListboxRowAnimationSetter)element.GetValue(RowAnimationSetterProperty);
        }
    }
}