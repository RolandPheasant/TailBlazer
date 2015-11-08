using System;
using System.IO;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Views
{
    /// <summary>
    /// Factory to create FileTailerViewModel. 
    /// Yes I know this can be resolved in di using explicit args but they are fragile due to having to know field names.
    /// </summary>
    public class FileTailerViewModelFactory
    {

        private readonly IObjectProvider _objectProvider;

        public FileTailerViewModelFactory(IObjectProvider objectProvider)
        {
            _objectProvider = objectProvider;
        }

        public FileTailerViewModel Create(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
            return new FileTailerViewModel(_objectProvider.Get<ILogger>(), _objectProvider.Get<ISchedulerProvider>(), fileInfo);
        }
    }
}