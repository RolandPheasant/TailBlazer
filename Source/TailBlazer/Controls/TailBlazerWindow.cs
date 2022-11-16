using System.Windows;
using MaterialDesignExtensions.Controls;

namespace TailBlazer.Controls;

public class TailBlazerWindow : MaterialWindow
{
    static TailBlazerWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TailBlazerWindow), new FrameworkPropertyMetadata(typeof(TailBlazerWindow)));
    }

    public static readonly DependencyProperty LeftHeaderContentProperty = DependencyProperty.Register(
        "LeftHeaderContent", typeof(object), typeof(TailBlazerWindow), new PropertyMetadata(default(object)));

    public static void SetLeftHeaderContent(DependencyObject element, object value)
    {
        element.SetValue(LeftHeaderContentProperty, value);
    }

    public static object GetLeftHeaderContent(DependencyObject element)
    {
        return (object)element.GetValue(LeftHeaderContentProperty);
    }

    public static readonly DependencyProperty RightHeaderContentProperty = DependencyProperty.Register(
        "RightHeaderContent", typeof(object), typeof(TailBlazerWindow), new PropertyMetadata(default(object)));

    public static void SetRightHeaderContent(DependencyObject element, object value)
    {
        element.SetValue(RightHeaderContentProperty, value);
    }

    public static object GetRightHeaderContent(DependencyObject element)
    {
        return (object)element.GetValue(RightHeaderContentProperty);
    }
}