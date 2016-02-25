using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Kernel;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Domain.StateHandling
{

    public class StateBucketService:IDisposable, IStateBucketService
    {
        private readonly IDisposable _cleanUp;

        private const string SettingStoreKey = "StateBucket";
        private readonly ISourceCache<StateBucket, StateBucketKey> _cache = new SourceCache<StateBucket, StateBucketKey>(s=>s.Key);
  

        public StateBucketService(ISettingsStore store)
        {
            var converter = new StateBucketConverter();

            bool loading = false;
            var writer = _cache.Connect()
                .ToCollection()
                .Select(buckets => converter.Convert(buckets.ToArray()))
                .Subscribe(state =>
                {
                    if (loading) return;
                    store.Save(SettingStoreKey, state);
                });


            //TODO: Make this error proof

            var initialState = store.Load(SettingStoreKey);
            var initialBuckets = converter.Convert(initialState);

            try
            {
                loading = true;
                _cache.AddOrUpdate(initialBuckets);
            }
            finally
            {
                loading = false;
            }

            _cleanUp = new CompositeDisposable(writer, _cache);
        }
        
        public void Write(string type, string id, State state)
        {
            _cache.AddOrUpdate(new StateBucket(type, id, state,DateTime.Now));
        }

        public Optional<State> Lookup(string type, string id)
        {
            return _cache.Lookup(new StateBucketKey(type, id))
                .Convert(container=>container.State);
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}