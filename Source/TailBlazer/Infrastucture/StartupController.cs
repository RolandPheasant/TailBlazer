using System;
using System.Reflection;
using System.Windows.Threading;
using TailBlazer.Domain.FileHandling.Recent;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Settings;
using TailBlazer.Views;

namespace TailBlazer.Infrastucture
{
    public class StartupController
    {
        public StartupController(IObjectProvider objectProvider,IObjectRegister register,
            ILogger logger)
        {

            logger.Info($"Starting Tail Blazer version v{Assembly.GetEntryAssembly().GetName().Version}");
            logger.Info($"at {DateTime.Now}");


            //run start up jobs
            objectProvider.Get<FileHeaderNamingJob>();
            objectProvider.Get<UhandledExceptionHandler>();

            var settingsRegister = objectProvider.Get<ISettingsRegister>();
            settingsRegister.Register(new GeneralOptionsConverter(), "GeneralOptions");
            settingsRegister.Register(new RecentFilesToStateConverter(), "RecentFiles");
            logger.Info("Starting complete");

        }
    }

}
