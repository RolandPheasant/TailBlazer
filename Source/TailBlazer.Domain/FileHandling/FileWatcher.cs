using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{


    public class FileWatcher : IFileWatcher
    {

        public IObservable<FileNotification> Latest { get; }

        private FileInfo FileInfo { get;  }

        public string FullName => FileInfo.FullName;

        public string Name => FileInfo.Name;
        
        public string Folder => FileInfo.DirectoryName;

        public FileWatcher([NotNull] FileInfo fileInfo, IScheduler scheduler=null)
        {
            FileInfo = fileInfo;
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
            Latest = fileInfo.WatchFile(scheduler: scheduler ?? Scheduler.Default)
                .DistinctUntilChanged()
                .TakeWhile(notification => notification.Exists).Repeat()
                ;//.Replay(1).RefCount();
        }
    }
}