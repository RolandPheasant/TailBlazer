using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace TailBlazer.Views.Tail
{
    /// <summary>
    /// Interaction logic for TailView.xaml
    /// </summary>
    public partial class TailView : UserControl
    {
        public TailView()
        {
            InitializeComponent();
            Loaded += (sender, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {

                    this.SearchTextBox.Focus();
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                }));


            };
        }
    }
}
