using System;

namespace TailBlazer.Domain.FileHandling
{
    public interface IFileWatcher
    {
        IObservable<FileNotification> Latest { get; }
        IObservable<FileStatus> Status { get; }
        string FullName { get; }
        string Name { get; }
        string Folder { get; }
    }
}