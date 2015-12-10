using System;
using System.IO;
using System.Reactive.Concurrency;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    public class FileTailerFactory : IFileTailerFactory
    {
        private readonly ILogFactory _logFactory;

        public FileTailerFactory(ILogFactory logFactory)
        {
            _logFactory = logFactory;
        }

        public FileTailer Create(FileInfo file, IObservable<ILineProvider> filter, IObservable<ScrollRequest> scrollRequest, IScheduler scheduler = null)
        {
            return new FileTailer(file, filter,
                scrollRequest,
                _logFactory.Create<FileTailer>());
        }
    }
}