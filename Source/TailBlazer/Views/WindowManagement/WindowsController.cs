using DynamicData;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastructure;

namespace TailBlazer.Views.WindowManagement;

public class WindowsController : IWindowsController, IDisposable
{
    private readonly ILogger _logger;
    private readonly ISourceCache<HeaderedView, Guid> _views = new SourceCache<HeaderedView, Guid>(vc=>vc.Id);

    public IObservableCache<HeaderedView, Guid> Views { get; }

    public WindowsController(ILogger logger)
    {
        _logger = logger;
        Views = _views.AsObservableCache();
    }

    public void Register(HeaderedView item)
    {
        _views.AddOrUpdate(item);
    }

    public void Remove(HeaderedView item)
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