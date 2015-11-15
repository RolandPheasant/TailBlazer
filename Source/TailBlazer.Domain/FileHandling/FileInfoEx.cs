using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using DynamicData.Kernel;

namespace TailBlazer.Domain.FileHandling
{
    public struct LineIndex : IEquatable<LineIndex>
    {
        private readonly int _line;
        private readonly int _index;

        public LineIndex(int line, int index)
        {
            _line = line;
            _index = index;
        }

        public int Line => _line;

        public int Index => _index;

        #region Equality

        public bool Equals(LineIndex other)
        {
            return _line == other._line && _index == other._index;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is LineIndex && Equals((LineIndex) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_line*397) ^ _index;
            }
        }

        public static bool operator ==(LineIndex left, LineIndex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LineIndex left, LineIndex right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return $"{Line} (index={Index})";
        }
    }

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

                         var reader = new StreamReader(stream, Encoding.Default, true);
                         string line;

                         var notifier = source
                              .Where(n => n.NotificationType == FileNotificationType.Changed)
                             .StartWith(createdNotification)
                             .Scan((FileScanResult)null, (state, notification) =>
                             {
                                 var count = state?.TotalLines ?? 0;
                                 var index = state?.Index + 1 ?? 0;
                                 var previousCount = count;
                                 var previousItems = state?.MatchingLines ?? new LineIndex[0];
                                 var newItems = new List<LineIndex>();


                                 var indexInResult = previousItems.Length;

                                 while ((line = reader.ReadLine()) != null)
                                 {
                                    
                                     if (predicate == null)
                                     {
                                 
                                         count++;
                                         newItems.Add(new LineIndex(count, count));
                                         indexInResult++;
                                     }
                                     else
                                     {
                                         count++;
                                         if (!predicate(line)) continue;
                                      
                                         newItems.Add(new LineIndex(count, indexInResult));
                                         indexInResult++;
                                     }
                                 }

                                 //combine the 2 arrays
                                 var newLines = new LineIndex[previousItems.Length + newItems.Count];
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

        public static IEnumerable<T> ReadLines<T>(this FileInfo source, LineIndex[] lines, Func<LineIndex,string, T>  selector)
        {
           
            var cached = lines.ToDictionary(l => l.Line);

            using (var stream = File.Open(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream, Encoding.Default, true))
                {
                    string line;
                    int position = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        position++;
                        var contained = cached.Lookup(position);

                        if (contained.HasValue)
                            yield return selector(contained.Value, line);

                    }
                }
            }
        }

    }
}
