using System;
using System.Windows;
using System.Windows.Threading;
using StructureMap;
using TailBlazer.Views;

namespace TailBlazer.Infrastucture
{
    public class BootStrap
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new App { ShutdownMode = ShutdownMode.OnLastWindowClose };
            app.InitializeComponent();


            var tempWindowToGetDispatcher  = new MainWindow();

           var container =  new Container(x=> x.AddRegistry<AppRegistry>());

           container.Configure(x => x.For<Dispatcher>().Add(tempWindowToGetDispatcher.Dispatcher));
            var factory = container.GetInstance<WindowFactory>();
            var window = factory.Create();

            //run start up jobs
            container.GetInstance<FileHeaderNamingJob>();
            tempWindowToGetDispatcher.Close();
            window.Show();
             app.Run();
        }
    }
}
