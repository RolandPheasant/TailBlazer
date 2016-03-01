using System;
using System.IO;
using StructureMap;
using StructureMap.Configuration.DSL;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Views;
using TailBlazer.Views.Tail;
using ILogger = TailBlazer.Domain.Infrastructure.ILogger;

namespace TailBlazer.Infrastucture
{
    internal class AppRegistry : Registry
    {
        public AppRegistry()
        {
            //set up logging
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");
            if (!File.Exists(path))
            {
                // should use the default config which is a resource
                using (var stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(TailBlazer.Properties.Resources.log4net)))
                {
                    log4net.Config.XmlConfigurator.Configure(stream);
                }
            }
            else
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(path));
            }
            For<ILogger>().Use<Log4NetLogger>().Ctor<Type>("type").Is(x => x.ParentType).AlwaysUnique();

            For<ISelectionMonitor>().Use<SelectionMonitor>();
            For<ISearchInfoCollection>().Use<SearchInfoCollection>();
            For<ISearchMetadataCollection>().Use<SearchMetadataCollection>().Transient();
            
            For<ITextFormatter>().Use<TextFormatter>();

            For<ISettingsStore>().Use<FileSettingsStore>().Singleton();
            For<IFileWatcher>().Use<FileWatcher>();


            For<UhandledExceptionHandler>().Singleton();
            For<ObjectProvider>().Singleton();
            Forward<ObjectProvider, IObjectProvider>();
            Forward<ObjectProvider, IObjectRegister>();

            //TODO: Account for there being multiple ViewModelFactories- probably need a register
            For<ViewFactoryService>().Singleton();
            Forward<ViewFactoryService, IViewFactoryRegister>();
            Forward<ViewFactoryService, IViewFactoryProvider>();

            For<TailViewModelFactory>().Singleton();
          //  Forward<TailViewModelFactory, IViewModelFactory>();

            Scan(scanner =>
            {
                scanner.ExcludeType<ILogger>();

                //to do, need a auto-exclude these from AppConventions
                scanner.ExcludeType<SelectionMonitor>();
                scanner.ExcludeType<SearchInfoCollection>();
                scanner.ExcludeType<SearchMetadataCollection>();
                scanner.ExcludeType<ITextFormatter>();
                scanner.ExcludeType<ViewFactoryService>();

                scanner.ExcludeType<FileWatcher>();
                scanner.LookForRegistries();
                scanner.Convention<AppConventions>();

                scanner.AssemblyContainingType<ILogFactory>();
                scanner.AssemblyContainingType<AppRegistry>();
            });
        }

    }
}

