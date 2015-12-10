using System;
using System.IO;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;


namespace TailBlazer.Views
{
    /// <summary>
    /// Factory to create FileTailerViewModel. 
    /// Yes I know this can be resolved in di using explicit args but they are fragile due to having to know field names.
    /// </summary>
    public class FileTailerViewModelFactory
    {

        private readonly IObjectProvider _objectProvider;
        private readonly ILogFactory _logFactory;

        public FileTailerViewModelFactory(IObjectProvider objectProvider, ILogFactory logFactory)
        {
            _objectProvider = objectProvider;
            _logFactory = logFactory;
        }

        public FileTailerViewModel Create(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            return new FileTailerViewModel(_logFactory.Create<FileTailerViewModel>(),
                _objectProvider.Get<ISchedulerProvider>(),
                fileInfo,
                _objectProvider.Get<IFileTailerFactory>(),
                _objectProvider.Get<SelectionMonitor>(),
                _objectProvider.Get<IClipboardHandler>(),
                _objectProvider.Get<ITailCollection>());
        }
    }
}