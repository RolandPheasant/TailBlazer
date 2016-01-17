using System;
using System.Linq;
using System.Reactive.Disposables;
using DynamicData;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Settings;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Recent
{
    public class RecentSearchCollection : IRecentSearchCollection, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IDisposable _cleanUp;
        private readonly  ISourceCache<RecentSearch, string> _files = new SourceCache<RecentSearch, string>(fi=>fi.Text);

        public IObservableList<RecentSearch> Items { get; }

        public RecentSearchCollection(ILogger logger, ISetting<RecentSearch[]> setting)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger;

            Items = _files.Connect()
                        .RemoveKey()
                        .AsObservableList();

            var loader = setting.Value.Subscribe(files =>
            {
                _files.Edit(innerCache =>
                {
                    //all files are loaded when state changes, so only add new ones
                    var newItems = files
                        .Where(f => !innerCache.Lookup(f.Text).HasValue)
                        .ToArray();

                    innerCache.AddOrUpdate(newItems);
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

        public void Add(RecentSearch file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            
            _files.AddOrUpdate(file);
        }

        public void Remove(RecentSearch file)
        {
            _files.Remove(file);

        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}