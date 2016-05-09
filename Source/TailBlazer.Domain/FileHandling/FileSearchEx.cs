using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public static class FileSearchEx
    {
        public static IObservable<FileSearchResult> Search(this FileInfo source, Func<string, bool> predicate, IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return source.WatchFile(scheduler: scheduler)

                .WithSegments()
                .Search(predicate, scheduler);
        }

        public static IObservable<FileSearchResult> Search(this IObservable<FileNotification> source, Func<string, bool> predicate, IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return source.WithSegments().Search(predicate, scheduler);
        }

        public static IObservable<FileSearchResult> Search(this IObservable<FileSegmentCollection> source, Func<string, bool> predicate, IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var published = source.Replay(1).RefCount();
            var nameChanged = published.Select(fsc => fsc.Info.Name).DistinctUntilChanged().Skip(1);
            var diff = published.Select(fsc => fsc.SizeDiff);

            var searcher = Observable.Create<FileSearchResult>(observer =>
            {
                var fileSearch = new FileSearcher(published, predicate, scheduler: scheduler);
                var publisher = fileSearch.SearchResult.SubscribeSafe(observer);
                return new CompositeDisposable(publisher, fileSearch);
            });

            var searchFactory = searcher.CombineLatest(diff, (search, sizeDiff) => new { search, sizeDiff });

            //this is the magic which allows the search to be recreated when a log file rolls
            return searchFactory
                .TakeUntil(nameChanged)
                .TakeWhile(x => x.sizeDiff >= 0).Repeat()
                .Select(x => x.search);
        }



        public static IEnumerable<T> SearchLines<T>(this StreamReaderExtended source,
            Func<string, bool> predicate,
            Func<long, T> selector,
            Func<string, long, bool> shouldBreak)
        {
            if (source.EndOfStream) yield break;

            long previousPostion = source.AbsolutePosition();

            string line;
            while ((line = source.ReadLine()) != null)
            {
                long position = source.AbsolutePosition();

                if (predicate(line))
                    yield return selector(previousPostion);

                if (shouldBreak(line, position))
                    yield break;

                previousPostion = position;
            }
        }

    }
}