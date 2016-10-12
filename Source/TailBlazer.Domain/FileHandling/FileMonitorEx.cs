using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public static class FileMonitorEx
    {
        //[NotNull]
        //IObservable<FileSegmentsWithTail> fileSegments,
        //    [NotNull] IObservable<ScrollRequest> scrollRequest,
        //    Func<string, bool> predicate = null,
        //    IScheduler scheduler = null



        //public static IObservable<ILineReader> Monitor([NotNull] this IObservable<FileSegmentsWithTail> source, 
        //    [NotNull] IObservable<ScrollRequest> scrollRequest, 
        //    Func<string, bool> filter = null, 
        //    IScheduler scheduler = null)
        //{
        //    if (source == null) throw new ArgumentNullException(nameof(source));
        //    if (scrollRequest == null) throw new ArgumentNullException(nameof(scrollRequest));

        //    return new FileMonitor(source, scrollRequest, filter, scheduler);
        //}



        //public static  IObservable<ILineReader> Monitor([NotNull] this IObservable<FileSegmentsWithTail> source, Func<string, bool> filter = null, IScheduler scheduler = null)
        //{
        //    if (source == null) throw new ArgumentNullException(nameof(source));

        //    if (filter == null)
        //        return new Indexer(source, scheduler: scheduler).Result;

        //    return new FileSearchIndexer(source, filter, scheduler: scheduler).SearchResult;
        //}

        public static IObservable<ILineReader> Monitor([NotNull] this IObservable<FileSegmentReport> source, Func<string, bool> filter = null, IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            if (filter == null)
                return new FileIndexer(source).Result;

            return Observable.Never<ILineReader>();
            //return new FileSearchIndexer(source, filter, scheduler: scheduler).SearchResult;
        }
    }
}