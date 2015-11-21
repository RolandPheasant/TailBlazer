using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using DynamicData.Kernel;

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
        public static IObservable<FileNotification> WatchFile(this FileInfo file, TimeSpan? refreshPeriod = null,IScheduler scheduler = null)
        {
            return Observable.Create<FileNotification>(observer =>
            {
                var refresh = refreshPeriod ?? TimeSpan.FromMilliseconds(250);
                scheduler = scheduler ?? Scheduler.Default;

                //TODO: create a cool-off period after a poll to account for over running jobs
                Func<IObservable<FileNotification>> poller = () => Observable.Interval(refresh, scheduler)
                      .StartWith(0)
                    // .ObserveOn(scheduler)
                    .Scan((FileNotification) null, (state, _) => state == null
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

        public static IObservable<FileNotification> WatchFile(this FileInfo file, IObservable<Unit> pulse)
        {
            return Observable.Create<FileNotification>(observer =>
            {
                //TODO: create a cool-off period after a poll to account for over running jobs
                Func<IObservable<FileNotification>> poller = () => pulse.StartWith(Unit.Default)
                    // .ObserveOn(scheduler)
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



        /// <summary>
        /// Indexes all the lines of the specified file notificationn
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static IObservable<LineIndicies> Index(this IObservable<FileNotification> source)
        {
            return source
                 .Where(n => n.NotificationType == FileNotificationType.Created)
                 .Select(createdNotification =>
                 {
                     return Observable.Create<LineIndicies>(observer =>
                     {
                         var indexer = new LineIndexer((FileInfo) createdNotification);

                         var notifier = source
                              .Where(n => n.NotificationType == FileNotificationType.Changed)
                             .StartWith(createdNotification)
                             .Scan((LineIndicies)null, (state, notification) =>
                             {
                                 var lines = indexer.ReadToEnd().ToArray();
                                 return new LineIndicies(lines, state);
                             })
                             .SubscribeSafe(observer);

                         return new CompositeDisposable(indexer, notifier);;
                     });
                 }).Switch();
        }

        public static IObservable<LineMatches> Match(this IObservable<FileNotification> source, Func<string,bool> predicate)
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
                                var lines = indexer.ScanToEnd().ToArray();
                                return new LineMatches(lines, state);
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
                using (var reader = new StreamReader(stream, Encoding.Default, true))
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

        public static IEnumerable<T> ReadLines<T>(this FileInfo source, IEnumerable<LineIndex> lines, Func<LineIndex,string, T>  selector)
        {
            using (var stream = File.Open(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {

                foreach (var lineIndex in lines.OrderBy(l=>l.Index))
                {
                    var data = new byte[lineIndex.Size-2];
                    stream.Seek(lineIndex.Start, SeekOrigin.Begin);
                    stream.Read(data, 0, data.Length);

                    var result = Encoding.Default.GetString(data);
                    yield return selector(lineIndex, result);
                }
            }
        }

    }
}
