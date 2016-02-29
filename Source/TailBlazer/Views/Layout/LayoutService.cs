using System;
using System.Xml.Linq;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Views.Layout
{
    public class LayoutService : ILayoutService
    {
        private readonly ISettingsStore _store;
        private readonly ILayoutConverter _converter;
        private readonly ILogger _logger;
        private const string LayoutName = "_LayoutName";

        public LayoutService(ISettingsStore store, ILayoutConverter converter, ILogger logger)
        {
            _store = store;
            _converter = converter;
            _logger = logger;
            Restore();
        }

        public void Write()
        {
            try
            {
                var xml = _converter.CaptureState();
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
                var element = XDocument.Parse(restored.Value);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,"Problem reading layout");
            }

        }

    }
}