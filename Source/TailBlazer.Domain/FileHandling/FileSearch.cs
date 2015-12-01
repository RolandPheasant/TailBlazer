using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public class FileSearch
    {
        public FileInfo Info { get;  }
        public Func<string, bool> Predicate { get; set; }
        public Encoding Encoding { get;  }

        public IObservable<bool> IsSearching { get; }

        private readonly IScheduler _scheduler;

        public FileSearch([NotNull] FileInfo info,
            [NotNull] Func<string, bool> predicate,
            IObservable<Unit> refresher,
            Encoding encoding = null, 
            IScheduler scheduler =null)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            Info = info;
            Predicate = predicate;
            Encoding = encoding ?? info.GetEncoding();
            _scheduler = scheduler ?? Scheduler.Default;
        }

        public IObservable<FileSearchResult> Search()
        {
            return Observable.Create<FileSearchResult>(observer =>
            {
                //1. Segment files: Scan tail first
                //2. Provide user feedback?

                _scheduler.Schedule(() =>
                {
                    var searchReslt = Search(0, (int)Info.Length);
                });

                return new CompositeDisposable();
            });
        }

        private FileSearchResult Search(int start, int end)
        {
            long lastPosition = 0;
            using (var stream = File.Open(Info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                long[] lines;
                using (var reader = new StreamReaderExtended(stream, Encoding, false))
                {
                    stream.Seek(start, SeekOrigin.Begin);
                    if (reader.EndOfStream)
                        return null;

                    lines = reader.SearchLines(Predicate, i => i, (line, position) =>
                    {
                        lastPosition = position;
                        return end != -1 && lastPosition >= end;

                    }).ToArray();
                }
                return new FileSearchResult(start, lastPosition, lines);
            }
        }
    }
}