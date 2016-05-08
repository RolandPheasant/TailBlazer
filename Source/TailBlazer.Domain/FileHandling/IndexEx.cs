using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public static class IndexEx
    {
        public static IObservable<ILineProvider> Index(this IObservable<FileSegmentCollection> source)
        {
            var published = source.Replay(1).RefCount();

            var nameChanged = published.Select(fsc => fsc.Info.Name).DistinctUntilChanged().Skip(1);
            var indexFactory = published
                .Publish(shared =>
                {
                    var diff = shared.Select(fsc => fsc.SizeDiff);
                    var idx = Observable.Create<IndexCollection>(observer =>
                    {
                        var indexer = new Indexer(shared);
                        var notifier = indexer.Result.SubscribeSafe(observer);
                        return new CompositeDisposable(indexer, notifier);
                    });

                    return idx.CombineLatest(diff, (index, sizeDiff) => new { index, sizeDiff });
                });

            //this is the beast which allows the indexer to be recreated when a log file rolls
            return indexFactory
                .TakeUntil(nameChanged)
                .TakeWhile(x => x.sizeDiff >= 0).Repeat()
                .Select(x => x.index);

        }

        public static IObservable<ILineProvider> Index(this IObservable<FileNotification> source)
        {
            return source.WithSegments().Index();
        }
    }
}