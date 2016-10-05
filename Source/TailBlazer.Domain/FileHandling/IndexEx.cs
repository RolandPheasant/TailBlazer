using System;
using System.Reactive.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public static class IndexEx
    {
        //public static IObservable<ILineProvider> Index(this FileInfo source,  IScheduler scheduler = null)
        //{
        //    if (source == null) throw new ArgumentNullException(nameof(source));

        //    return source.WatchFile(scheduler: scheduler)
        //        .WithSegments()
        //        .Index();
        //}

        public static IObservable<ILineProvider> Index(this IObservable<FileSegmentsWithTail> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var published = source.Replay(1).RefCount();
            var nameChanged = published.Select(fsc => fsc.Segments.Info.Name).DistinctUntilChanged().Skip(1);
            var diff = published.Select(fsc => fsc.Segments.SizeDiff);

            var idx = Observable.Create<IndexCollection>(observer =>
            {
                var indexer = new FileIndexer(published);
                return indexer.Result.SubscribeSafe(observer);
            });

            var searchFactory = idx.CombineLatest(diff, (search, sizeDiff) => new { search, sizeDiff });

            //this is the magic which allows the search to be recreated when a log file rolls
            return searchFactory
                .TakeUntil(nameChanged)
                .TakeWhile(x => x.sizeDiff >= 0).Repeat()
                .Select(x => x.search);

        }

        //public static IObservable<ILineProvider> Index(this IObservable<FileNotification> source)
        //{
        //    return source.WithSegments().Index();
        //}

        //public static IObservable<ILineProvider> Index(this IObservable<FileSegmentsWithTail> source)
        //{
        //    return Observable.Defer(() =>
        //    {
        //        return new FileIndexer(source).Result;
        //    });

        //}
    }
}