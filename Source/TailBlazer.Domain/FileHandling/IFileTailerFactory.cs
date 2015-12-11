using System;
using System.IO;
using System.Reactive.Concurrency;

namespace TailBlazer.Domain.FileHandling
{
    public interface IFileTailerFactory
    {
        LineScroller Create(FileInfo file, IObservable<ILineProvider> filter, IObservable<ScrollRequest> scrollRequest, IScheduler scheduler = null);
    }
}