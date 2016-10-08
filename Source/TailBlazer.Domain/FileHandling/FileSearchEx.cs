using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;

namespace TailBlazer.Domain.FileHandling
{
    public static class FileSearchEx
    {
        public static IObservable<FileSearchCollection> Search(this FileInfo source, Func<string, bool> predicate, IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return source.WatchFile(scheduler: scheduler)
                .WithSegments()
                .WithTail()
                .Search(predicate, scheduler);
        }

        //public static IObservable<FileSearchCollection> Search(this IObservable<FileNotification> source, Func<string, bool> predicate, IScheduler scheduler = null)
        //{
        //    if (source == null) throw new ArgumentNullException(nameof(source));
        //    if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        //    return source.WithSegments().Search(predicate, scheduler);
        //}

        public static IObservable<FileSearchCollection> Search(this IObservable<FileSegmentsWithTail> source, Func<string, bool> predicate, IScheduler scheduler = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return new FileSearchIndexer(source, predicate, scheduler: scheduler).SearchResult;
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