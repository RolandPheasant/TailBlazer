using System.Linq;
using System.Windows;

namespace TailBlazer.Infrastructure;

public delegate void ApplicationExitingDelegate();

public static class WindowAssist
{
    public static readonly DependencyProperty ApplicationClosingProperty = DependencyProperty.RegisterAttached("ApplicationClosing", typeof (ApplicationExitingDelegate), typeof (WindowAssist), 
        new PropertyMetadata(default(ApplicationExitingDelegate),OnClosingDelegateSet));

    public static void SetApplicationClosing(Window element, ApplicationExitingDelegate value)
    {
        element.SetValue(ApplicationClosingProperty, value);
    }

    public static ApplicationExitingDelegate GetApplicationClosing(Window element)
    {
        return (ApplicationExitingDelegate)element.GetValue(ApplicationClosingProperty);
    }

    public static void OnClosingDelegateSet(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var window = (Window) sender;
        var closingDelegate = args.NewValue as ApplicationExitingDelegate;

        window.Closing += (s, e) =>
        {
            var windows = Application.Current.Windows.OfType<MainWindow>().Count();
            if (windows == 1)
                closingDelegate?.Invoke();
        };
    }
}