using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Ratings;

namespace TailBlazer.Domain.FileHandling
{
    public sealed class FileWatcher : IFileWatcher, IDisposable
    {
        private readonly ISubject<long> _scanFrom = new BehaviorSubject<long>(0);

        private readonly IDisposable _cleanUp;
        private readonly IScheduler _scheduler;

        private FileInfo FileInfo { get; }
        private IObservable<FileSegmentReport> Segments { get; }

        public IObservable<FileStatus> Status { get; }
      
        public IObservable<long> Size { get; }
        
        public string FullName => FileInfo.FullName;

        public string Name => FileInfo.Name;
        
        public string Folder => FileInfo.DirectoryName;

        
        public FileWatcher([NotNull] FileInfo fileInfo, IRatingService ratingsMetrics, IScheduler scheduler=null)
        {
            _scheduler = scheduler ?? Scheduler.Default; ;
            FileInfo = fileInfo;
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            var refreshRate = ratingsMetrics.Metrics.Take(1)
                .Select(metrics=> TimeSpan.FromMilliseconds(metrics.RefreshRate))
                .Wait();
            
            //switch the file when it is rolled over, or when it is cleared
            var shared = _scanFrom.Select(start => start == 0
                ? fileInfo.WatchFile(scheduler: _scheduler, refreshPeriod: refreshRate)
                : fileInfo.WatchFile(scheduler: _scheduler, refreshPeriod: refreshRate).ScanFromEnd())
                .Switch()
                .DistinctUntilChanged()
                .Publish();
            
            Size = shared.Select(fn => fn.Size).Replay(1).RefCount();

            Segments = shared.MonitorChanges()
                .SegmentWithReport()
                .Replay(1).RefCount();

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

        public ILineMonitor Monitor(IObservable<ScrollRequest> scrollRequest, Func<string, bool> predicate = null, IScheduler scheduler = null)
        {
            return new FileMonitor(Segments, scrollRequest, predicate, scheduler: scheduler);
        }

        public ILineMonitor Monitor(IObservable<ScrollRequest> scrollRequest, IObservable<Func<string, bool>> predicateObs, IScheduler scheduler = null)
        {
            return new FileMonitor(Segments, scrollRequest, predicateObs: predicateObs, scheduler: scheduler);
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