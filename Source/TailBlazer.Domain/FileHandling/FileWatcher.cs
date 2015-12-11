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

        public FileWatcher([NotNull] FileInfo file, IScheduler scheduler=null)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            Latest = file.WatchFile(scheduler: scheduler ?? Scheduler.Default)
                .DistinctUntilChanged()
                .TakeWhile(notification => notification.Exists).Repeat()
                .Replay(1).RefCount();
        }
    }
}