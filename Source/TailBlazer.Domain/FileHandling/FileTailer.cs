using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using DynamicData.Kernel;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    public class FileTailer: IDisposable
    {
        private readonly IDisposable _cleanUp;
        public IObservable<int> TotalLines { get;  }
        public IObservable<int> MatchedLines { get; }
        public IObservable<long> FileSize { get; }
        public IObservableList<Line> Lines { get; }
        public IObservable<bool> IsSearching { get;  }
        public IObservable<bool> IsLoading { get; }


        public FileTailer(FileInfo file,
                IObservable<FileSearchResult> filter,
                IObservable<ScrollRequest> scrollRequest,
                ILogger logger,
                IScheduler scheduler = null)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.Info($"Constructing file tailer for {file.FullName}");

            var lines = new SourceList<Line>();
            Lines = lines.AsObservableList();

            var isBusy = new Subject<bool>();
            IsSearching = isBusy.AsObservable();

            var locker = new object();
            scrollRequest = scrollRequest.Synchronize(locker);
            
            var fileWatcher = file.WatchFile(scheduler: scheduler)
                            .DistinctUntilChanged()
                            .TakeWhile(notification => notification.Exists).Repeat()
                            .Replay(1).RefCount();

            var indexer = fileWatcher
                            .Index()
                            .Synchronize(locker)
                            .RetryWithBackOff((Exception error, int attempts) =>
                            {
                                //todo: plug in file missing or error into the screen
                                return TimeSpan.FromSeconds(1);
                            })
                            .Replay(1).RefCount();

            IsLoading = indexer.Take(1).Select(_ => false).StartWith(true);

            //count matching lines (all if no filter is specified)
            MatchedLines = indexer
                        .CombineLatest(filter, (i, f) => f == FileSearchResult.None ? i.Count : f.Count)
                        .Synchronize(locker);

            //count total line
            TotalLines = indexer.Select(x => x.Count);
            FileSize = fileWatcher.Select(notification => notification.Size);

            var aggregator = indexer.CombineLatest(filter, scrollRequest, (idx, mtch, scroll) => new CombinedResult(scroll, mtch, idx))
                .Select(result =>
                {
                    var scroll = result.Scroll;
                    var indicies = result.Incidies;
                    var matched = result.FilterResult;

                    var indices = result.FilterResult  == FileSearchResult.None
                                    ? indicies.GetIndicies(scroll)
                                    : matched.GetIndicies(scroll,indicies);

                    var currentPage = indices.ToArray();
                    var previous = lines.Items.Select(l => l.LineIndex).ToArray();
                    var removed = previous.Except(currentPage, LineIndex.LineComparer).ToArray();
                    var removedLines = lines.Items.Where(l => removed.Contains(l.LineIndex)).ToArray();

                    var added = currentPage.Except(previous, LineIndex.LineComparer).ToArray();
                    //finally we can load the line from the file
                    var newLines = file.ReadLine(added, (lineIndex, text) =>
                    {
                        var isEndOfTail = indicies.ChangedReason == LinesChangedReason.Tailed && lineIndex.Line > indicies.TailStartsAt;
                        return new Line(lineIndex, text, isEndOfTail ? DateTime.Now : (DateTime?)null);
                    }, indicies.Encoding).ToArray();

                    return new { NewLines = newLines, OldLines = removedLines };
                })

                 .Where(fn => fn.NewLines.Length + fn.OldLines.Length > 0)
                .Subscribe(changes =>
                {
                    //update observable list
                    lines.Edit(innerList =>
                    {
                        if (changes.OldLines.Any()) innerList.RemoveMany(changes.OldLines);
                        if (changes.NewLines.Any()) innerList.AddRange(changes.NewLines);
                    });
                });
            _cleanUp = new CompositeDisposable(Lines, lines, aggregator, Disposable.Create(() => isBusy.OnCompleted()));
        }


        private class CombinedResult
        {
            public ScrollRequest Scroll { get;  }
            public FileSearchResult FilterResult { get;  }
            public IIndexCollection Incidies { get;  }

            public CombinedResult(ScrollRequest scroll, FileSearchResult filterResult, IIndexCollection incidies)
            {
                Scroll = scroll;
                FilterResult = filterResult;
                Incidies = incidies;
            }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
