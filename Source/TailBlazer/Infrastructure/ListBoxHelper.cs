using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TailBlazer.Infrastructure;

public  class ListBoxHelper
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
        var receiver = args.NewValue as IAttachedListBox;
        receiver?.Receive((ListBox)sender);
    }

}