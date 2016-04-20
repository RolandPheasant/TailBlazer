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
        bool tailIsChecked;


        public TailView()
        {
            InitializeComponent();
            tailIsChecked = TailCheckBox.IsChecked.Value;

            this.KeyDown += new KeyEventHandler(SpacebarIsPressed);
            IsVisibleChanged += (sender, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {

                    this.SearchTextBox.Focus();
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                }));


            };
        }
        void SpacebarIsPressed (object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                TailCheckBox.IsChecked = tailIsChecked;
                tailIsChecked = !TailCheckBox.IsChecked.Value;
            }
        }
        
    }
}
