using System;
using DynamicData;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views
{
    public class WindowsController : IWindowsController, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ISourceCache<ViewContainer, Guid> _views = new SourceCache<ViewContainer, Guid>(vc=>vc.Id);

        public IObservableCache<ViewContainer, Guid> Views { get; }

        public WindowsController(ILogger logger)
        {
            _logger = logger;
            Views = _views.AsObservableCache();
        }

        public void Register(ViewContainer item)
        {
            _views.AddOrUpdate(item);
        }

        public void Remove(ViewContainer item)
        {
            _views.Remove(item);
        }

        public void Remove(Guid id)
        {
            _views.Remove(id);
        }

        public void Dispose()
        {
            _views.Dispose();
            Views.Dispose();
        }
    }
}