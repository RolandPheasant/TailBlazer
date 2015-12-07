using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public static class IndexEx
    {
        public static IObservable<IIndexCollection> Index(this IObservable<FileSegmentCollection> source)
        {
            var indexFactory = source
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
                .TakeWhile(x => x.sizeDiff >= 0).Repeat()
                .Select(x => x.index);

        }
        public static IObservable<IIndexCollection> Index(this IObservable<FileNotification> source)
        {
            return source.WithSegments().Index();
        }

        public static IEnumerable<T> ScanLines<T>(this StreamReaderExtended source,
        int compression,
        Func<long, T> selector,
        Func<string, long, bool> shouldBreak)
        {

            int i = 0;
            if (source.EndOfStream) yield break;

            string line;
            while ((line = source.ReadLine()) != null)
            {
                i++;
                var position = source.AbsolutePosition();

                if (i == compression)
                {
                    yield return selector(position);
                    i = 0;
                };

                if (shouldBreak(line, position))
                    yield break;
            }
        }

    }
}