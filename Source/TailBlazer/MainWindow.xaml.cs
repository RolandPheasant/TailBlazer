using System.ComponentModel;
using MahApps.Metro.Controls;
using TailBlazer.Views.WindowManagement;

namespace TailBlazer
{

    public delegate void WindowClosing();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        public MainWindow()
        {
            InitializeComponent();

            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {

            var windowsModel = DataContext as WindowViewModel;
            windowsModel?.OnWindowClosing();
        }
    }
}
