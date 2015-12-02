using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using DynamicData;
using DynamicData.Kernel;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    public static class FileInfoEx
    {
        /// <summary>
        /// A simpler alternative to the irritatingly useless FileSystemWatcher
        /// </summary>
        /// <param name="file">The file to monitor</param>
        /// <param name="refreshPeriod">The refresh period.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <returns></returns>
        public static IObservable<FileNotification> WatchFile(this FileInfo file, TimeSpan? refreshPeriod = null,
            IScheduler scheduler = null)
        {
           return Observable.Create<FileNotification>(observer =>
            {
                var refresh = refreshPeriod ?? TimeSpan.FromMilliseconds(250);
                scheduler = scheduler ?? Scheduler.Default;

                FileNotification notification = null;
                return scheduler.ScheduleRecurringAction(refresh, () =>
                {
                    try
                    {
                        notification = notification == null
                            ? new FileNotification(file)
                            : new FileNotification(notification);

                        observer.OnNext(notification);
                    }
                    catch (Exception ex)
                    {
                        notification = new FileNotification(file, ex);
                        observer.OnNext(notification);
                    }
                });

            }).DistinctUntilChanged();
        }


        public static IObservable<FileNotification> WatchFile(this FileInfo file, IObservable<Unit> pulse)
        {
            return Observable.Create<FileNotification>(observer =>
            {
                 Func<IObservable<FileNotification>> poller = () => pulse.StartWith(Unit.Default)

                    .Scan((FileNotification)null, (state, _) => state == null
                       ? new FileNotification(file)
                       : new FileNotification(state))
                    .DistinctUntilChanged();

                /*
                    In theory, poll merrily away except slow down when there is an error.
                */
                return poller()
                    .Catch<FileNotification, Exception>(ex => Observable.Return(new FileNotification(file, ex))
                        .Concat(poller().DelaySubscription(TimeSpan.FromSeconds(10))))
                    .SubscribeSafe(observer);
            });
        }

        //public static DynamicFileSegments Dynamic(this IObservable<FileSegments> source)
        //{
        //    return new DynamicFileSegments(source);
        //}

        public static IObservable<FileSegments> WithSegments(this IObservable<FileNotification> source)
        {
            return source
                 .Where(n => n.NotificationType == FileNotificationType.Created)
                 .Select(createdNotification =>
                 {
                     return Observable.Create<FileSegments>(observer =>
                     {
                         var refresher = source
                             .Where(n => n.NotificationType == FileNotificationType.Changed)
                             .StartWith(createdNotification)
                             .ToUnit();

                         var indexer = new FileSegmenter((FileInfo)createdNotification, refresher);
                         return indexer.Segments.SubscribeSafe(observer);
                     });
                 }).Switch();
        }

        public static IObservable<IIndexCollection> IndexSparsely(this IObservable<FileSegments> source)
        {
            return Observable.Defer(() =>
            {
                return Observable.Create<SparseIndexCollection>(observer =>
                {
                    var indexer = new SparseIndexer2(source);
                    var notifier = indexer.Result.SubscribeSafe(observer);
                    return new CompositeDisposable(indexer, notifier);
                });
            });

        }

        public static IObservable<IIndexCollection> IndexSparsely(this IObservable<FileNotification> source)
        {
            return source.WithSegments().IndexSparsely();
        }

        public static IObservable<LineMatches> Match(this IObservable<FileNotification> source, Func<string,bool> predicate, Action<bool> isSearching=null)
        {
            return source
                .Where(n => n.NotificationType == FileNotificationType.Created)
                .Select(createdNotification =>
                {
                    return Observable.Create<LineMatches>(observer =>
                    {
                        var indexer = new LineMatcher((FileInfo) createdNotification, predicate);

                        var notifier = source
                            .Where(n => n.NotificationType == FileNotificationType.Changed)
                            .StartWith(createdNotification)
                            .Scan((LineMatches) null, (state, notification) =>
                            {
                                var shouldNotifyOfSearch = isSearching != null &&
                                            notification.NotificationType == FileNotificationType.Created;
                                try
                                {
                                    if (shouldNotifyOfSearch) isSearching(true);
                                    var lines = indexer.ScanToEnd().ToArray();
                                    return new LineMatches(lines, state);
                                }
                                finally
                                {
                                    if (shouldNotifyOfSearch) isSearching(false);
                                }
                            })
                            .SubscribeSafe(observer);

                        return new CompositeDisposable(indexer, notifier);
                    });
                }).Switch();
        }

        public static IEnumerable<Line> ReadLines(this FileInfo source, int[] lines, Func<int, bool> isEndOfTail = null)
        {
            using (var stream = File.Open(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var reader = new StreamReaderExtended(stream, Encoding.Default, true))
                {
                    string line;
                    int position = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        position++;

                        if (lines.Contains(position))
                            yield return new Line(position,line, isEndOfTail==null ? null :(isEndOfTail(position)? DateTime.Now : (DateTime?)null));
                    }
                }
            }
        }

        public static long FindNextEndOfLinePosition(this FileInfo source, long initialPosition)
        {
            using (var stream = File.Open(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                stream.Seek(initialPosition, SeekOrigin.Begin);
                using (var reader = new StreamReaderExtended(stream, Encoding.Default, true))
                {
                    if (reader.EndOfStream) return -1;
                    reader.ReadLine();
                    return reader.AbsolutePosition();
                }
            }
        }

        public static long FindNextEndOfLinePosition(this StreamReaderExtended source, long initialPosition,
            SeekOrigin origin= SeekOrigin.Current)
        {

            if (source.EndOfStream) return -1;
            source.ReadLine();
            return source.AbsolutePosition();
        }

        public static long GetFileLength(this FileInfo source)
        {
            using (var stream = File.Open(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                return stream.Length;
            }
        }
    }
}
