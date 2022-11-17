using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace TailBlazer.Views.Tail;

/// <summary>
/// Interaction logic for TailView.xaml
/// </summary>
public partial class TailView : UserControl
{
    public TailView()
    {
        InitializeComponent();
        IsVisibleChanged += (sender, e) =>
        {
            FocusSearchTextBox();
        };            
    }

    private void FocusSearchTextBox()
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            SearchTextBox.Focus();
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }));
    }

    private void ApplicationCommandFind_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        FocusSearchTextBox();
    }
}