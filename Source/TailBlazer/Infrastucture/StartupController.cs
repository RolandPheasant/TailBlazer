using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TailBlazer.Domain.FileHandling.Recent;
using TailBlazer.Domain.FileHandling.TextAssociations;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Domain.StateHandling;
using TailBlazer.Infrastucture.AppState;
using TailBlazer.Views.Formatting;
using TailBlazer.Views.Layout;
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

            logger.Info($"Starting Tail Blazer version v{Assembly.GetEntryAssembly().GetName().Version}");
            logger.Info($"at {DateTime.Now}");

            var settingsRegister = objectProvider.Get<ISettingsRegister>();
            settingsRegister.Register(new GeneralOptionsConverter(), "GeneralOptions");
            settingsRegister.Register(new RecentFilesToStateConverter(), "RecentFiles");
            settingsRegister.Register(new StateBucketConverter(), "BucketOfState");
            settingsRegister.Register(new RecentSearchToStateConverter(), "RecentSearch");
            settingsRegister.Register(new TextAssociationToStateConverter(), "TextAssociation");
            
            //run start up jobs
            objectProvider.Get<FileHeaderNamingJob>();
            objectProvider.Get<UhandledExceptionHandler>();
            objectProvider.Get<ThemeSetterJob>();

            //TODO: Need type scanner then this code is not required
            var viewFactoryRegister = objectProvider.Get<IViewFactoryRegister>();
            viewFactoryRegister.Register<TailViewModelFactory>();


           logger.Info("Starting complete");

        }
    }

}
