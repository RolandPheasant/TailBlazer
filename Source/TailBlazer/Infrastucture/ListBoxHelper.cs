using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TailBlazer.Infrastucture
{
    public static class ListBoxHelper
    {
        public static readonly DependencyProperty SelectionMonitorProperty = DependencyProperty.RegisterAttached("SelectionMonitor", typeof(IAttachedListBox), typeof(ListBoxHelper),
            new PropertyMetadata(default(IAttachedListBox), PropertyChanged));

        public static void SetSelectionMonitor(Selector element, IAttachedListBox value)
        {
            element.SetValue(SelectionMonitorProperty, value);
        }

        public static IAttachedListBox GetSelectionMonitor(Selector element)
        {
            return (IAttachedListBox)element.GetValue(SelectionMonitorProperty);
        }

        public static void PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var receiver = args.NewValue as IAttachedListBox;
            receiver?.Receive((ListBox)sender);
        }
    }
}