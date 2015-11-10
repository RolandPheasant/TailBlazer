using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData.Kernel;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    public static class FileInfoEx
    {
        public static IObservable<FileNotification> WatchFile(this FileInfo file, TimeSpan? refreshPeriod = null,
            IScheduler scheduler = null)
        {
            return Observable.Create<FileNotification>(observer =>
            {
                var refresh = refreshPeriod ?? TimeSpan.FromMilliseconds(250);
                scheduler = scheduler ?? Scheduler.Default;

                //TODO: create a cool-off period after a poll to account for over running jobs
                Func<IObservable<FileNotification>> poller = () => Observable.Interval(refresh, scheduler)
                                        .Scan((FileNotification)null, (state, _) =>
                                        {
                                            return state == null
                                                ? new FileNotification(file)
                                                : new FileNotification(state);
                                        }).DistinctUntilChanged();

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
        /// Produces an observable report of lines in the file which matches the specified predicate, together with meta data to
        /// assist reading the actual file lines
        /// If no predicate is supplied all lines are returned.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public static IObservable<FileScanResult> ScanFile(this IObservable<FileNotification> source, Func<string, bool> predicate = null)
        {
            return source
                 .Where(n => n.NotificationType == FileNotificationType.Created)
                 .Select(createdNotification =>
                 {
                     return Observable.Create<FileScanResult>(observer =>
                     {
                         var stream = File.Open(createdNotification.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite);
                         stream.Seek(0, SeekOrigin.Begin);

                         var reader = new StreamReader(stream);
                         string line;

                         var notifier = source
                              .Where(n => n.NotificationType == FileNotificationType.Changed)
                             .StartWith(createdNotification)
                             .Scan((FileScanResult)null, (state, notification) =>
                             {
                                 var count = state?.TotalLines ?? 0;
                                 var index = state?.Index + 1 ?? 0;
                                 var previousCount = count;
                                 var previousItems = state?.MatchingLines ?? new int[0];
                                 var newItems = new List<int>();

                                 while ((line = reader.ReadLine()) != null)
                                 {
                                     if (predicate == null)
                                     {
                                         count++;
                                         newItems.Add(count);
                                     }
                                     else
                                     {
                                         count++;
                                         if (!predicate(line)) continue;
                                         newItems.Add(count);
                                     }
                                 }

                                 //combine the 2 arrays
                                 var newLines = new int[previousItems.Length + newItems.Count];
                                 previousItems.CopyTo(newLines, 0);
                                 newItems.CopyTo(newLines, previousItems.Length);
                                 
                                 return new FileScanResult(notification, newLines, count, previousCount, index);
                             })
                             .SubscribeSafe(observer);


                         return Disposable.Create(() =>
                         {
                             notifier.Dispose();
                             stream.Close();
                             stream.Dispose();
                             reader.Close();
                             reader.Dispose();
                         });
                     });
                 }).Switch();
        }


        /// <summary>
        /// Produces an observable array of lines in the file which matches the specified predicate.
        /// If no predicate is supplied all lines are returned.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="endOfTailChanged">The end of tail changed.</param>
        /// <returns></returns>
        public static IObservable<int[]> ScanLineNumbers(this FileInfo file, Func<string, bool> predicate = null, Action<int> endOfTailChanged = null)
        {

            return Observable.Create<int[]>(observer =>
            {
                var stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite);
                stream.Seek(0, SeekOrigin.Begin);

                var reader = new StreamReader(stream);
                string line;

                var monitor = file.WatchFile()
                    .Where(e => e.NotificationType==FileNotificationType.Created || e.NotificationType == FileNotificationType.Changed)
                    .ToUnit()
                    .StartWithUnit()
                    .Scan(Tuple.Create(new ImmutableList<int>(), 0), (state, _) =>
                    {


                        var i = state.Item2;

                        //notify  
                        endOfTailChanged?.Invoke(i);

                        var newItems = new List<int>();
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (predicate == null)
                            {
                                i++;
                                newItems.Add(i);
                            }
                            else
                            {
                                i++;
                                if (!predicate(line)) continue;
                                newItems.Add(i);

                            }
                        }
                        var result = state.Item1.Add(newItems.ToArray());


                        return Tuple.Create(result, i);
                    }).Select((tuple,index) => tuple.Item1.Data)
                    .SubscribeSafe(observer);

                return Disposable.Create(() =>
                {
                    monitor.Dispose();
                    stream.Close();
                    stream.Dispose();
                    reader.Close();
                    reader.Dispose();
                });
            });

        }

        public static IEnumerable<Line> ReadLines(this FileInfo source, int[] lines, Func<int, bool> isEndOfTail = null)
        {
            using (var stream = File.Open(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream))
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



    }
}
