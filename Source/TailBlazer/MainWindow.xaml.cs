using System.ComponentModel;
using TailBlazer.Controls;
using TailBlazer.Views.WindowManagement;

namespace TailBlazer
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : TailBlazerWindow
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
