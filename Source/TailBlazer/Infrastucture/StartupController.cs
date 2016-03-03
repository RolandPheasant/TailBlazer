using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using TailBlazer.Domain.FileHandling.Recent;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Domain.StateHandling;
using TailBlazer.Infrastucture.AppState;
using TailBlazer.Views.Options;
using TailBlazer.Views.Recent;
using TailBlazer.Views.Tail;
using TailBlazer.Views.WindowManagement;

namespace TailBlazer.Infrastucture
{
    public class StartupController
    {
        public StartupController(IObjectProvider objectProvider,ILogger logger, IApplicationStatePublisher applicationStatePublisher)
        {
            applicationStatePublisher.Publish(ApplicationState.Startup);

            //Observable.FromEventPattern<CancelEventHandler, CancelEventArgs>(
            //            h => Application.Current.MainWindow.Closing += h,
            //            h => Application.Current.MainWindow.Closing -= h)
            //            .Subscribe(_ =>
            //            {
            //                applicationStatePublisher.Publish(ApplicationState.ShuttingDown);
            //                applicationStatePublisher.Publish(ApplicationState.ShutDown);
            //            });


            //Observable.FromEventPattern<ExitEventHandler, ExitEventArgs>(
            //            h => Application.Current.Exit += h,
            //            h => Application.Current.Exit -= h)
            //            .Subscribe(_ =>
            //            {
            //                applicationStatePublisher.Publish(ApplicationState.ShuttingDown);
            //                applicationStatePublisher.Publish(ApplicationState.ShutDown);
            //            });



            logger.Info($"Starting Tail Blazer version v{Assembly.GetEntryAssembly().GetName().Version}");
            logger.Info($"at {DateTime.Now}");


            //run start up jobs
            objectProvider.Get<FileHeaderNamingJob>();
            objectProvider.Get<UhandledExceptionHandler>();

            var settingsRegister = objectProvider.Get<ISettingsRegister>();
            settingsRegister.Register(new GeneralOptionsConverter(), "GeneralOptions");
            settingsRegister.Register(new RecentFilesToStateConverter(), "RecentFiles");
            settingsRegister.Register(new StateBucketConverter(), "BucketOfState");
            settingsRegister.Register(new RecentSearchToStateConverter(), "RecentSearch");
            logger.Info("Starting complete");


            //TODO: Need type scanner then this code is not required
            var viewFactoryRegister = objectProvider.Get<IViewFactoryRegister>();
            viewFactoryRegister.Register<TailViewModelFactory>();


        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           
        }

        private void Current_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            throw new NotImplementedException();
        }
    }

}
