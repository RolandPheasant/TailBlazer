using System;
using System.Collections;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views
{
    public class FileHeaderNamingJob: IDisposable
    {
        private readonly IDisposable _cleanUp;

        public FileHeaderNamingJob(IWindowsController windowsController)
        {
            if (windowsController == null) throw new ArgumentNullException(nameof(windowsController));

            var filePages = windowsController.Views
                .Connect(vc => vc.Header is FileHeader)
                .Transform(vc => (FileHeader) vc.Header)
                .AsObservableCache();

            var added = filePages.Connect()
                .Subscribe(_ =>
                {
                    var tree = new FileNamer(filePages.Items.Select(f=>f.FullName));
                    filePages.Items.Select(page => new
                    {
                        Item = page,
                        Label = tree.GetName(page.FullName)
                    })
                    .ForEach(x=> x.Item.DisplayName = x.Label);
                });

            _cleanUp  = new CompositeDisposable(filePages, added);
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }



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