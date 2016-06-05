﻿using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using StructureMap;
using TailBlazer.Infrastucture;
using TailBlazer.Infrastucture.AppState;
using TailBlazer.Views.Layout;
using TailBlazer.Views.WindowManagement;

namespace TailBlazer
{
    public class BootStrap
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [STAThread]
        public static void Main(string[] args)
        {
            FreeConsole();

            var app = new App { ShutdownMode = ShutdownMode.OnLastWindowClose };
            app.InitializeComponent();

            var tempWindowToGetDispatcher = new Window();

            var container = new Container(x => x.AddRegistry<AppRegistry>());
            container.Configure(x => x.For<Dispatcher>().Add(tempWindowToGetDispatcher.Dispatcher));
            container.GetInstance<StartupController>();

            var factory = container.GetInstance<WindowFactory>();
            var window = factory.Create(args);
            tempWindowToGetDispatcher.Close();

            var layoutServce = container.GetInstance<ILayoutService>();

            var appStatePublisher = container.GetInstance<IApplicationStatePublisher>();
            app.Exit += (sender, e) => appStatePublisher.Publish(ApplicationState.ShuttingDown);
            window.Show();
            app.Run();
        }
    }
}
