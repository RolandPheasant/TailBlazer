using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TailBlazer.Infrastucture
{
    public static class ListBoxHelper
    {
        public static readonly DependencyProperty SelectionMonitorProperty = DependencyProperty.RegisterAttached("SelectionMonitor", typeof(ISelectionMonitor), typeof(ListBoxHelper),
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
            var receiver = args.NewValue as ISelectionMonitor;
            receiver?.Receive((ListBox)sender);
        }
    }
}