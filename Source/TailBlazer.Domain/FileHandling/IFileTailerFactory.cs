using System;
using System.IO;
using System.Reactive.Concurrency;

namespace TailBlazer.Domain.FileHandling
{
    public interface IFileTailerFactory
    {
        FileTailer Create(FileInfo file,IObservable<string> textToMatch,IObservable<ScrollRequest> scrollRequest, IScheduler scheduler = null);
    }
}