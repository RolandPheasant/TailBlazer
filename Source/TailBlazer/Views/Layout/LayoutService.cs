using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Xml.Linq;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Infrastucture.AppState;

namespace TailBlazer.Views.Layout
{
    public class LayoutService : ILayoutService
    {
        private readonly ISettingsStore _store;
        private readonly ILogger _logger;
        private readonly IObjectProvider _objectProvider;
        private const string LayoutName = "_LayoutName";

        public LayoutService(ISettingsStore store, 
            ILogger logger,
            ISchedulerProvider schedulerProvider,
            IObjectProvider objectProvider,
            IApplicationStateNotifier stateNotifier)
        {
            _store = store;
            _logger = logger;
            _objectProvider = objectProvider;
            schedulerProvider.MainThread.Schedule(Restore);

            stateNotifier.StateChanged.Where(state => state == ApplicationState.ShuttingDown)
                .Subscribe(_ =>
                {
                    Write();
                });
        }

        public void Write()
        {
            try
            {
                var converter = _objectProvider.Get<ILayoutConverter>();
                var xml = converter.CaptureState();
                _store.Save(LayoutName, new State(1, xml.ToString()));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Problem loading layout");
            }
        }

        public void Restore()
        {
            try
            {
                var restored = _store.Load(LayoutName);
                if (restored == State.Empty)
                    return;

                var element = XDocument.Parse(restored.Value);

                var converter = _objectProvider.Get<ILayoutConverter>();
                converter.Restore(element.Root);

            }
            catch (Exception ex)
            {
                _logger.Error(ex,"Problem reading layout");
            }

        }

    }
}