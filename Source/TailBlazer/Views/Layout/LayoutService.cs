using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Views.WindowManagement;
using System.Reactive.Concurrency;

namespace TailBlazer.Views.Layout
{
    public class LayoutService : ILayoutService
    {
        private readonly ISettingsStore _store;
        private readonly IWindowFactory _windowFactory;
        // private readonly ILayoutConverter _converter;
        private readonly ILogger _logger;
        private readonly IObjectProvider _objectProvider;
        private const string LayoutName = "_LayoutName";

        public LayoutService(ISettingsStore store, 
            IWindowFactory windowFactory, 
            ILogger logger,
            ISchedulerProvider schedulerProvider,
            IObjectProvider objectProvider)
        {
            _store = store;
            _windowFactory = windowFactory;
            // _converter = converter;
            _logger = logger;
            _objectProvider = objectProvider;

            //Task.Factory.StartNew(Restore);
            schedulerProvider.MainThread.Schedule(Restore);
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