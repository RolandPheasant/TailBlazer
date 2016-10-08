using System;
using System.Reactive.Concurrency;

namespace TailBlazer.Domain.FileHandling
{
    public static class FileMonitorEx
    {
        public static  IObservable<ILineReader> Monitor(this IObservable<FileSegmentsWithTail> source, Func<string, bool> filter = null, IScheduler scheduler = null)
        {
            if (filter == null)
                return new Indexer(source, scheduler: scheduler).Result;

            return new FileSearchIndexer(source, filter, scheduler: scheduler).SearchResult;
        }
    }
}