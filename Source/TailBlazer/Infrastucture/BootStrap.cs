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


           var container =  new Container(x=> x.AddRegistry<AppRegistry>());
           var factory = container.GetInstance<WindowFactory>();
           var window = factory.Create();
           container.Configure(x => x.For<Dispatcher>().Add(window.Dispatcher));


            //run start up jobs

            window.Show();
             app.Run();
        }
    }
}
