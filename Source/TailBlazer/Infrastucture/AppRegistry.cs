using System;
using System.IO;
using StructureMap.Configuration.DSL;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Infrastructure;
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
                throw new FileNotFoundException("The log4net.config file was not found" + path);

            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(path));
            For<ILogger>().Use<Log4NetLogger>().Ctor<Type>("type").Is(x => x.ParentType).AlwaysUnique();

            For<ISelectionMonitor>().Use<SelectionMonitor>();
            For<ISearchInfoCollection>().Use<SearchInfoCollection>();
            
           For<IFileWatcher>().Use<FileWatcher>();
            Scan(scanner =>
            {
                scanner.ExcludeType<ILogger>();

                //to do, need a auto-exclude these AppConventions
                scanner.ExcludeType<SelectionMonitor>();
                scanner.ExcludeType<SearchInfoCollection>();
                scanner.ExcludeType<FileWatcher>();

                //  scanner.ExcludeType<ISelectionMonitor>();
                scanner.LookForRegistries();
                scanner.Convention<AppConventions>();

                scanner.AssemblyContainingType<ILogFactory>();
                scanner.AssemblyContainingType<AppRegistry>();
            });
        }
    }
}

