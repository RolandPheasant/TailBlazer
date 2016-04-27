using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData.Kernel;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    class FileRewriter
    {
        public IObservable<FileNotification> Notifications { get; }

        public FileRewriter([NotNull] IObservable<FileNotification> fileWatcher, long startFrom, TimeSpan? refreshPeriod=null, IScheduler scheduler=null)
        {
            if (fileWatcher == null) throw new ArgumentNullException(nameof(fileWatcher));
            
            //TODO: Do we need to specifically handle errors?
            Notifications = Observable.Create<FileNotification>(observer =>
            {
                var newFile =  Path.GetTempFileName();
                var info = new FileInfo(newFile);
                var resultStream = info.WatchFile(refreshPeriod,scheduler).SubscribeSafe(observer);

                var fileWriter = fileWatcher.Scan(new FileReadResult(Enumerable.Empty<string>(), startFrom), (state, notification) =>
                {
                    return ReadLines(notification.FullName, state.EndPosition);
                }).Subscribe(result =>
                {
                   //Write lines to the file
                    var lines = result.Lines.AsArray();

                    if (lines.Any())
                        File.WriteAllLines(newFile, lines);
                });
                
                return Disposable.Create(() =>
                {
                    fileWriter.Dispose();
                    resultStream.Dispose();
                    File.Delete(newFile);
                });
            });
        }

        private FileReadResult ReadLines(string file, long firstPosition)
        {
            var list = new List<string>();
            long endPosition;
            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var reader = new StreamReaderExtended(stream))
                {
                    //go to starting point
                    stream.Seek(firstPosition, SeekOrigin.Begin);

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        list.Add(line);
                    }
                    endPosition = reader.AbsolutePosition();
                }
            }
            return new FileReadResult(list, endPosition);
        }
        
        private class FileReadResult
        {
            public IEnumerable<string> Lines { get;  }
            public long EndPosition { get;  }

            public FileReadResult(IEnumerable<string> lines,long endPosition)
            {
                Lines = lines;
                EndPosition = endPosition;
            }
        }
    }
}
