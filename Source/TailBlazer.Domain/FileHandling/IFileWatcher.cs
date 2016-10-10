using System;
using System.Reactive.Concurrency;

namespace TailBlazer.Domain.FileHandling
{
    public interface IFileWatcher
    {
        IObservable<FileStatus> Status { get; }
        IObservable<FileSegmentsWithTail> Segments { get; }
        IObservable<long> Size { get; }

        ILineMonitor Monitor(IObservable<ScrollRequest> scrollRequest, Func<string, bool> predicate = null, IScheduler scheduler = null);
        ILineMonitor Monitor(IObservable<ScrollRequest> scrollRequest, IObservable<Func<string, bool>> predicate, IScheduler scheduler=null);

        IObservable<ILineProvider> Search(Func<string, bool> predicate);
        IObservable<ILineProvider> Index();

        string FullName { get; }
        string Name { get; }
        string Folder { get; }

        void ScanFrom(long scanFrom);
        void Clear();
        void Reset();
    }
}