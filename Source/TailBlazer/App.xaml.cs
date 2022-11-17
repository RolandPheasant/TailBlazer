using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using StructureMap;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;
using TailBlazer.Infrastucture.AppState;
using TailBlazer.Views.Layout;
using TailBlazer.Views.WindowManagement;

using System.Reactive.Concurrency;



namespace TailBlazer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeConsole();

    protected override void OnStartup(StartupEventArgs e)
    {
        FreeConsole();

        var tempWindowToGetDispatcher = new Window();

        var container = new Container(x => x.AddRegistry<AppRegistry>());
        container.Configure(x => x.For<Dispatcher>().Add(tempWindowToGetDispatcher.Dispatcher));
        container.GetInstance<StartupController>();

        var factory = container.GetInstance<WindowFactory>();
        var window = factory.Create(e.Args);
        tempWindowToGetDispatcher.Close();

        var layoutServce = container.GetInstance<ILayoutService>();
        var scheduler = container.GetInstance<ISchedulerProvider>();
        scheduler.MainThread.Schedule(window.Show);

        var appStatePublisher = container.GetInstance<IApplicationStatePublisher>();
        Exit += (sender, e) => appStatePublisher.Publish(ApplicationState.ShuttingDown);

        base.OnStartup(e);
    }

}