using System;
using System.Windows;
using System.Windows.Threading;
using StructureMap;
using TailBlazer.Infrastucture;
using TailBlazer.Views;
using TailBlazer.Views.WindowManagement;

namespace TailBlazer
{
    public class BootStrap
    {
        [STAThread]
        public static void Main(string[] args)
        {

            var app = new App { ShutdownMode = ShutdownMode.OnLastWindowClose };
            app.InitializeComponent();


           var tempWindowToGetDispatcher = new MainWindow();

            var container = new Container(x => x.AddRegistry<AppRegistry>());
            container.Configure(x => x.For<Dispatcher>().Add(tempWindowToGetDispatcher.Dispatcher));
            container.GetInstance<StartupController>();

            var factory = container.GetInstance<WindowFactory>();

            var window = factory.Create(args);


            tempWindowToGetDispatcher.Close();
            window.Show();
            app.Run();
        }
    }
}
