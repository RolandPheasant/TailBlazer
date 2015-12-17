using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using DynamicData;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Domain.FileHandling
{

    public class RecentFile
    {
        public DateTime Timestamp { get; }
        public string  Name  { get; }

        public RecentFile(FileInfo fileInfo)
        {
            Name = fileInfo.FullName;
            Timestamp = DateTime.Now;
        }

        public RecentFile(DateTime timestamp, string name)
        {
            Timestamp = timestamp;
            Name = name;
        }
    }

    public class RecentFiles : IRecentFiles, IDisposable
    {
        private const string SettingsKey = "RecentFiles";

        private readonly ILogger _logger;
        private readonly IDisposable _cleanUp;
        private readonly  ISourceCache<RecentFile, string> _files = new SourceCache<RecentFile, string>(fi=>fi.Name);

        public IObservableList<RecentFile> Items { get; }

        public RecentFiles(ILogger logger, ISettingFactory settingFactory, ISettingsStore store)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (store == null) throw new ArgumentNullException(nameof(store));
            _logger = logger;

            //TODO:  create specialist object so we can sort / pin and timestamp etc
            Items = _files.Connect()
                        .RemoveKey()
                        .AsObservableList();

            var setting = settingFactory.Create(new RecentFilesToStateConverter(), SettingsKey);

            var loader = setting.Value.Subscribe(files =>
            {
                _files.Edit(innerCache =>
                {
                    //all files are loaded when state changes, so only add new ones
                    //var newItems = files
                    //    .Where(f => !innerCache.Lookup(f.FullName).HasValue)
                    //    .ToArray();

                    //innerCache.AddOrUpdate(newItems);
                });     
            });
       
            var settingsWriter = _files.Connect()
                .ToCollection()
                .Subscribe(items =>
                {
                    setting.Write(items.ToArray());
                });

            _cleanUp = new CompositeDisposable(settingsWriter, loader, _files,Items);
        }

        public void Register(FileInfo file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            
            _files.AddOrUpdate(new RecentFile(file));
        }

        public void Remove(FileInfo file)
        {
            _files.Remove(file.Name);

        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}