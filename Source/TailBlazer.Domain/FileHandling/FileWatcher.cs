using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
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

        private readonly ISubject<long> _scanFrom = new BehaviorSubject<long>(0);

        public FileWatcher([NotNull] FileInfo fileInfo, IScheduler scheduler=null)
        {
            FileInfo = fileInfo;
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            scheduler = scheduler ?? Scheduler.Default;
            
            var shared = _scanFrom.Select(start => start == 0
                ? fileInfo.WatchFile(scheduler: scheduler)
                : fileInfo.WatchFile(scheduler: scheduler).ScanFromEnd())
                .Switch();

            Latest = shared
                .TakeWhile(notification => notification.Exists).Repeat();
                //.Replay(1).RefCount();
                        

            Status = fileInfo.WatchFile(scheduler: scheduler).Select(notificiation =>
            {
                if (!notificiation.Exists || notificiation.Error != null)
                    return FileStatus.Error;

                return FileStatus.Loaded;
            })
            .StartWith(FileStatus.Loading)
            .DistinctUntilChanged();
        }

        public void ScanFrom(long scanFrom)
        {
            _scanFrom.OnNext(scanFrom);
        }

        public void Clear()
        {
            _scanFrom.OnNext(FileInfo.Length);
        }
        
        public void Reset()
        {
            _scanFrom.OnNext(0);
        }
    }
}