using System;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.Settings
{
    public class SettingFactory : ISettingFactory
    {
        private readonly IObjectProvider _objectProvider;
        private readonly ILogFactory _logFactory;
        private readonly ISettingsStore _settingsStore;

        public SettingFactory(IObjectProvider objectProvider, 
            ILogFactory logFactory,
            ISettingsStore settingsStore)
        {
            if (objectProvider == null) throw new ArgumentNullException(nameof(objectProvider));
            if (logFactory == null) throw new ArgumentNullException(nameof(logFactory));

            _objectProvider = objectProvider;
            _logFactory = logFactory;
            _settingsStore = settingsStore;
        }

        public ISetting<T> Create<T>(IConverter<T> converter, string key)
        {
            //TODO: Cache stored setting and retrive if required elsewhere
            var setting = new Setting<T>(_logFactory.Create<T>(),_settingsStore, converter,key);

            return setting;
        }

    }
}