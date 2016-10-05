using System;

namespace TailBlazer.Domain.FileHandling
{
    public interface IFileWatcher
    {
        IObservable<FileStatus> Status { get; }
        IObservable<FileSegmentsWithTail> Segments { get; }
        IObservable<long> Size { get; }

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