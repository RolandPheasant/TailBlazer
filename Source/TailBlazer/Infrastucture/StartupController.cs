using System;
using System.Reflection;
using TailBlazer.Domain.FileHandling.Recent;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Domain.StateHandling;
using TailBlazer.Views.Options;
using TailBlazer.Views.Recent;
using TailBlazer.Views.Searching;
using TailBlazer.Views.Tail;
using TailBlazer.Views.WindowManagement;

namespace TailBlazer.Infrastucture
{
    public class StartupController
    {
        public StartupController(IObjectProvider objectProvider,ILogger logger)
        {

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
    }

}
