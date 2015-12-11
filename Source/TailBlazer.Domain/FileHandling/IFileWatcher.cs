using System;

namespace TailBlazer.Domain.FileHandling
{
    public interface IFileWatcher
    {
        IObservable<FileNotification> Latest { get; }
    }
}