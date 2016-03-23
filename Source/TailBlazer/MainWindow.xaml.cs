using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using TailBlazer.Views.WindowManagement;

namespace TailBlazer
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        public MainWindow()
        {
            InitializeComponent();
          //  EventManager.RegisterClassHandler(typeof(Window), Mouse.MouseWheelEvent, new MouseEventHandler(MouseMouveEventHandler), true);

            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            var windowsModel = DataContext as WindowViewModel;
            windowsModel?.OnWindowClosing();
        }
    }
}
