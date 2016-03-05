using System;
using System.Windows;
using System.Windows.Threading;
using StructureMap;
using TailBlazer.Infrastucture;
using TailBlazer.Infrastucture.AppState;
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

            var tempWindowToGetDispatcher = new Window();

            var container = new Container(x => x.AddRegistry<AppRegistry>());
            container.Configure(x => x.For<Dispatcher>().Add(tempWindowToGetDispatcher.Dispatcher));
            container.GetInstance<StartupController>();

            var factory = container.GetInstance<WindowFactory>();
            var window = factory.Create(args);
            tempWindowToGetDispatcher.Close();

            var appStatePublisher = container.GetInstance<IApplicationStatePublisher>();
            app.Exit += (sender, e) => appStatePublisher.Publish(ApplicationState.ShuttingDown);
            window.Show();
            app.Run();
        }

        private static void App_Exit(object sender, ExitEventArgs e)
        {

        }
    }
}
