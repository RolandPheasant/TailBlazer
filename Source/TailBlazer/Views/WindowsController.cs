using System;
using DynamicData;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views
{
    public class WindowsController : IWindowsController
    {
        private readonly ISourceCache<ViewContainer, Guid> _views = new SourceCache<ViewContainer, Guid>(vc=>vc.Id);

        public WindowsController()
        {
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
    }
}