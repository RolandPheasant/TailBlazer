using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    public static class FileInfoEx
    {
        public static IObservable<int> CountLines(this FileInfo file, Func<string, bool> predicate = null)
        {

            return Observable.Create<int>(observer =>
            {
                var stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read,FileShare.Delete | FileShare.ReadWrite);
                stream.Seek(0, SeekOrigin.Begin);

                var reader = new StreamReader(stream);
                string line;

                Func<int> countToEnd = () =>
                {
                    var i = 0;
                    while ((line = reader.ReadLine())!=null)
                    {
                        if (predicate == null)
                        {
                            i++;
                        }
                        else
                        {
                            if (predicate(line)) i ++;
                        }
                    }
                    return i;
                };
                
                var monitor = file.WatchFile()
                    .Where(e => e.ChangeType == WatcherChangeTypes.Changed)
                    .ToUnit()
                    .StartWithUnit()
                    .Scan(countToEnd(), (total, _) => total + countToEnd())
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

        public static IObservable<FileSystemEventArgs> WatchFile(this FileInfo file)
        {
            //TODO: Create a return parameter signature which allows for all events to be monitors in one observable

            return Observable.Create<FileSystemEventArgs>(observer =>
            {

                var watcher = new FileSystemWatcher(file.DirectoryName, file.Name);
                watcher.EnableRaisingEvents = true;

                var changed = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>
                    (h => watcher.Changed += h, h => watcher.Changed -= h)
                    .Select(ev => ev.EventArgs);

                var deleted = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>
                    (h => watcher.Deleted += h, h => watcher.Deleted -= h)
                    .Select(ev => ev.EventArgs);

                var created = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>
                    (h => watcher.Created += h, h => watcher.Created -= h)
                    .Select(ev => ev.EventArgs);

                var renamed = Observable.FromEventPattern<RenamedEventHandler, RenamedEventArgs>
                        (h => watcher.Renamed += h, h => watcher.Renamed -= h)
                        .Select(ev => ev.EventArgs);

                return new CompositeDisposable(watcher, changed.Merge(created).Merge(deleted).SubscribeSafe(observer));

            });
        }

    }
}
