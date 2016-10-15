using System;
using System.Collections.Generic;
using System.IO;
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
        public static IObservable<FileChanges> MonitorChanges(this IObservable<FileNotification> source)
        {
            return source
                //.Where(notification => notification.Exists)
                .Scan((FileChanges) null, (state, latest) =>
                {
                    if (state==null)
                        return new FileChanges(latest);

                    return new FileChanges(state, latest);
                });
        }

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
        }



        //FileMonitor = shared.Scan((FileNameAndSize) null, (state, latest) =>
        //{
        //    return state == null ? new FileNameAndSize(latest) : new FileNameAndSize(state, latest);
        //}).Publish().RefCount();




        /// <summary>
        /// Determines the encoding of a file
        /// </summary>
        /// <returns></returns>
        public static Encoding GetEncoding(this IFileMetrics source)
        {
            using (var stream = File.Open(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var reader = new StreamReaderExtended(stream, true))
                {
                    return reader.CurrentEncoding;
                }
            }
        }


        /// <summary>
        /// Finds the delimiter by looking for the first line in the file and comparing chars
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static int FindDelimiter(this FileInfo source)
        {
            using (var stream = File.Open(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var reader = new StreamReaderExtended(stream, Encoding.Default, true))
                {
                    if (reader.EndOfStream)
                        return -1;
                    do
                    {
                        var ch = (char)reader.Read();

                        // Note the following common line feed chars: 
                        // \n - UNIX   \r\n - DOS   \r - Mac 
                        switch (ch)
                        {
                            case '\r':
                                var next = (char)reader.Peek();
                                //with \n is WINDOWS delimiter. Otherwise mac
                                return next == '\n' ? 2 : 1;
                            case '\n':
                                return 1;
                        }
                    } while (!reader.EndOfStream);
                    return -1;
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
