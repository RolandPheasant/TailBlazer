using System;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Ratings;

namespace TailBlazer.Domain.FileHandling
{
    public sealed class FileWatcher : IFileWatcher, IDisposable
    {
        public IObservable<FileStatus> Status { get; }
        public IObservable<FileSegmentsWithTail> Segments { get; }
        public IObservable<long> Size { get; }
        public IObservable<Unit> Invalidated { get; }

        public IObservable<FileNameAndSize> FileMonitor { get; }

        private FileInfo FileInfo { get;  }

        public string FullName => FileInfo.FullName;

        public string Name => FileInfo.Name;
        
        public string Folder => FileInfo.DirectoryName;

        public string Extension => FileInfo.Extension;

        private readonly ISubject<long> _scanFrom = new BehaviorSubject<long>(0);

        private readonly IDisposable _cleanUp;

        public FileWatcher([NotNull] FileInfo fileInfo, IRatingService ratingsMetrics, IScheduler scheduler=null)
        {
            FileInfo = fileInfo;
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            scheduler = scheduler ?? Scheduler.Default;

            var refreshRate = ratingsMetrics.Metrics.Take(1)
                .Select(metrics=> TimeSpan.FromMilliseconds(metrics.RefreshRate))
                .Wait();

            //switch the file when it is rolled over, or when it is cleared
            var shared = _scanFrom.Select(start => start == 0
                ? fileInfo.WatchFile(scheduler: scheduler, refreshPeriod: refreshRate)
                : fileInfo.WatchFile(scheduler: scheduler, refreshPeriod: refreshRate).ScanFromEnd())
                .DistinctUntilChanged()
                .Switch()
                .TakeWhile(notification => notification.Exists)
                .Repeat()
                .Publish();

            FileMonitor = shared.Scan((FileNameAndSize) null, (state, latest) =>
            {
                return state == null ? new FileNameAndSize(latest) : new FileNameAndSize(state, latest);
            }).Publish().RefCount();

            //file is invalidated at roll-over and and when the file is cleared
            Invalidated = shared.Scan((FileNameAndSize) null, (state, latest) =>
                {
                    return state == null ? new FileNameAndSize(latest) : new FileNameAndSize(state, latest);
                })
                .Select(nameAndSize => nameAndSize.Invalidated)
                .Where(invalidated => invalidated)
                .Select(x=> x)
                .ToUnit()
                .Publish().RefCount();

            Size = shared.Select(fn => fn.Size).Replay(1).RefCount();

            Segments = shared.WithSegments().WithTail()
                .TakeUntil(Invalidated)
                .Repeat()
                .Replay(1).RefCount()
                .Select(x => x);

            //FileNameChanged = Segments.Select(fsc => fsc.Segments.Info.Name).DistinctUntilChanged().Skip(1);
            //SizeDiff = Segments.Select(fsc => fsc.Segments.SizeDiff);

            Status = fileInfo.WatchFile(scheduler: scheduler).Select(notificiation =>
            {
                if (!notificiation.Exists || notificiation.Error != null)
                    return FileStatus.Error;

                return FileStatus.Loaded;
            })
            .StartWith(FileStatus.Loading)
            .DistinctUntilChanged();


            _cleanUp = shared.Connect();
        }
        

        public IObservable<ILineProvider> Index()
        {
            return Segments.Index();
        }

        public IObservable<ILineProvider> Search([NotNull] Func<string, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return Segments.Search(predicate);
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

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}