using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    [Flags]
    public enum FileStatus
    {
        Loading,
        Error,
        Loaded
    }

    public class FileWatcher : IFileWatcher
    {
        public IObservable<FileStatus> Status { get; }

        public IObservable<FileNotification> Latest { get; }

        private FileInfo FileInfo { get;  }

        public string FullName => FileInfo.FullName;

        public string Name => FileInfo.Name;
        
        public string Folder => FileInfo.DirectoryName;

        public string Extension => FileInfo.Extension;

        public FileWatcher([NotNull] FileInfo fileInfo, IScheduler scheduler=null)
        {
            FileInfo = fileInfo;
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            var shared = fileInfo.WatchFile(scheduler: scheduler ?? Scheduler.Default);
                        //.Replay(1)
                        //.RefCount();

            Latest = shared
                        .TakeWhile(notification => notification.Exists)
                        .Repeat();

            Status = shared.Select(notificiation =>
            {
                if (!notificiation.Exists || notificiation.Error != null)
                    return FileStatus.Error;

                return FileStatus.Loaded;
            })
            .StartWith(FileStatus.Loading)
            .DistinctUntilChanged();
        }
    }
}