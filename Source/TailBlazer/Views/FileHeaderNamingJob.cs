using System;
using System.Collections;
using System.Linq;
using DynamicData;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views
{
    public class FileHeaderNamingJob: IDisposable
    {
        private readonly IDisposable _cleanUp;

        public FileHeaderNamingJob(IWindowsController windowsController)
        {
            if (windowsController == null) throw new ArgumentNullException(nameof(windowsController));

            _cleanUp = windowsController.Views
                .Connect(vc => vc.Header is FileHeader)
                .Transform(vc => (FileHeader) vc.Header)
                .ToCollection()
                .Subscribe(files =>
                {
                    var renamer = new FileNamer(files.Select(f => f.FullName));
                    files.Select(page => new
                    {
                        Item = page,
                        Label = renamer.GetName(page.FullName)
                    })
                    .ForEach(x => x.Item.DisplayName = x.Label);
                });
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}