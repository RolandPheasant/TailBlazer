
using System;
using System.Reflection;
using TailBlazer.Domain.FileHandling.Recent;
using TailBlazer.Domain.FileHandling.TextAssociations;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Domain.StateHandling;
using TailBlazer.Infrastucture.AppState;
using TailBlazer.Views.Formatting;
using TailBlazer.Views.Options;
using TailBlazer.Views.Recent;
using TailBlazer.Views.Tail;
using TailBlazer.Views.WindowManagement;

namespace TailBlazer.Infrastucture
{
    public class StartupController
    {
        public StartupController(IObjectProvider objectProvider, ILogger logger,
            IApplicationStatePublisher applicationStatePublisher)
        {
            applicationStatePublisher.Publish(ApplicationState.Startup);

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
            settingsRegister.Register(new TextAssociationToStateConverter(), "TextAssociation");

            //TODO: Need type scanner then this code is not required
            var viewFactoryRegister = objectProvider.Get<IViewFactoryRegister>();
            viewFactoryRegister.Register<TailViewModelFactory>();

            objectProvider.Get<ThemeSetterJob>();

            logger.Info("Starting complete");
        }
    }
}