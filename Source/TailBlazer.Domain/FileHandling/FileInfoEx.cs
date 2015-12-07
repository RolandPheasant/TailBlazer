using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
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
            return pulse.StartWith(Unit.Default)

                .Scan((FileNotification) null, (state, _) => state == null
                    ? new FileNotification(file)
                    : new FileNotification(state));
            // .DistinctUntilChanged();
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

        public static IEnumerable<Line> ReadLinesByPosition(this FileInfo source, long[] positions, Func<int, bool> isEndOfTail = null)
        {
            using (var stream = File.Open(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var reader = new StreamReaderExtended(stream, Encoding.Default, true))
                {
                    foreach (var position in positions)
                    {
                        if (reader.AbsolutePosition() != position)
                        {
                            reader.DiscardBufferedData();
                            stream.Seek(position, SeekOrigin.Begin);

                        }
                        var line = reader.ReadLine();
                        yield return new Line((int)position, line,null);
                    }
                }
            }
        }
        
        public static long FindNextEndOfLinePosition(this StreamReaderExtended source, long initialPosition,
            SeekOrigin origin= SeekOrigin.Begin)
        {

            if (source.EndOfStream) return -1;
            source.BaseStream.Seek(initialPosition, origin);
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
