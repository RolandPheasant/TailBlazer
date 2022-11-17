using System;
using System.IO;
using StructureMap;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Infrastructure.AppState;
using TailBlazer.Infrastructure.KeyboardNavigation;
using TailBlazer.Views.Options;
using TailBlazer.Views.Tail;
using ILogger = TailBlazer.Domain.Infrastructure.ILogger;

namespace TailBlazer.Infrastructure;

internal class AppRegistry : Registry
{
    public AppRegistry()
    {
        //set up logging
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");
        if (!File.Exists(path))
        {
            // should use the default config which is a resource
            using var stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(TailBlazer.Properties.Resources.log4net));
            log4net.Config.XmlConfigurator.Configure(stream);
        }
        else
        {
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(path));
        }
        For<ILogger>().Use<Log4NetLogger>().Ctor<Type>("type").Is(x => x.ParentType).AlwaysUnique();

        For<ISelectionMonitor>().Use<SelectionMonitor>();
        For<ISearchInfoCollection>().Use<SearchInfoCollection>();
        For<ISearchMetadataCollection>().Use<SearchMetadataCollection>().Transient();
        For<ICombinedSearchMetadataCollection>().Use<CombinedSearchMetadataCollection>().Transient();
             

        For<ITextFormatter>().Use<TextFormatter>().Transient();
        For<ILineMatches>().Use<LineMatches>();
        For<ISettingsStore>().Use<FileSettingsStore>().Singleton();
        For<IFileWatcher>().Use<FileWatcher>();


        For<GeneralOptionsViewModel>().Singleton();
        For<UhandledExceptionHandler>().Singleton();
        For<ObjectProvider>().Singleton();
        Forward<ObjectProvider, IObjectProvider>();
        Forward<ObjectProvider, IObjectRegister>();


        For<ViewFactoryService>().Singleton();
        Forward<ViewFactoryService, IViewFactoryRegister>();
        Forward<ViewFactoryService, IViewFactoryProvider>();

        For<ApplicationStateBroker>().Singleton();
        Forward<ApplicationStateBroker, IApplicationStateNotifier>();
        Forward<ApplicationStateBroker, IApplicationStatePublisher>();

            
        For<TailViewModelFactory>().Singleton();

        For<IKeyboardNavigationHandler>().Use<KeyboardNavigationHandler>();

        Scan(scanner =>
        {
            scanner.ExcludeType<ILogger>();

            //to do, need a auto-exclude these from AppConventions
            scanner.ExcludeType<SelectionMonitor>();
            scanner.ExcludeType<SearchInfoCollection>();
            scanner.ExcludeType<SearchMetadataCollection>();
            scanner.ExcludeType<CombinedSearchMetadataCollection>();
            scanner.ExcludeType<TextFormatter>();
            scanner.ExcludeType<LineMatches>();
            scanner.ExcludeType<ViewFactoryService>();

                

            scanner.ExcludeType<FileWatcher>();
            scanner.LookForRegistries();
            scanner.Convention<AppConventions>();

            scanner.AssemblyContainingType<ILogFactory>();
            scanner.AssemblyContainingType<AppRegistry>();
        });
    }

}