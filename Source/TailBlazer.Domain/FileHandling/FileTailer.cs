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
            IObservable<string> textToMatch,
            IObservable<ScrollRequest> scrollRequest,
            ILogger logger ,
            IScheduler scheduler=null)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (textToMatch == null) throw new ArgumentNullException(nameof(textToMatch));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.Info($"Constructing file tailer for {file.FullName}");

            var lines = new SourceList<Line>();
            Lines = lines.AsObservableList();

            var isBusy = new Subject<bool>();
            IsSearching = isBusy.AsObservable();

            var locker = new object();
            scrollRequest = scrollRequest.Synchronize(locker);

            var matcher = textToMatch.Select(searchText =>
            {
                if (string.IsNullOrEmpty(searchText) || searchText.Length < 3)
                    return Observable.Return(LineMatches.None);

                return file.WatchFile(scheduler:scheduler)
                     .TakeWhile(notification => notification.Exists).Repeat()
                     .Match(s => s.Contains(searchText, StringComparison.OrdinalIgnoreCase), isBusy.OnNext);

            }).Switch()
            .Synchronize(locker)
            .Replay(1).RefCount();
            
            var fileWatcher = file.WatchFile(scheduler: scheduler)
                            .DistinctUntilChanged()
                            .TakeWhile(notification => notification.Exists).Repeat()
                            .Replay(1).RefCount();
            
            var indexer = fileWatcher
                            .IndexSparsely()
                            .Synchronize(locker)
                            .RetryWithBackOff((Exception error, int attempts) =>
                            {
                                //todo: plug in file missing or error into the screen
                                return TimeSpan.FromSeconds(1);
                            })
                            .Replay(1).RefCount();
            
            IsLoading = indexer.Take(1).Select(_=>false).StartWith(true);

            //count matching lines (all if no filter is specified)
            MatchedLines = indexer
                        .CombineLatest(matcher, (indicies, matches) => matches == LineMatches.None ? indicies.Count : matches.Count)
                        .Synchronize(locker);

            //count total line
            TotalLines = indexer.Select(x => x.Count);

            FileSize = fileWatcher.Select(notification => notification.Size);
            
            var aggregator = indexer.CombineLatest(matcher, scrollRequest,(idx, mtch, scroll) => new CombinedResult(scroll, mtch, idx))
                .Select(result =>
                {
                    var scroll = result.Scroll;
                    var indicies = result.Incidies;
                    var matched = result.MatchedLines;

                    var indices = result.MatchedLines.ChangedReason == LineMatchChangedReason.None 
                                    ? indicies.GetIndicies(scroll) 
                                    : indicies.GetIndicies(scroll,matched);

                    var currentPage = indices.ToArray();
                    var previous = lines.Items.Select(l => l.LineIndex).ToArray();
                    var removed = previous.Except(currentPage, LineIndex.LineComparer).ToArray();
                    var removedLines = lines.Items.Where(l=> removed.Contains(l.LineIndex)).ToArray();

                    var added = currentPage.Except(previous, LineIndex.LineComparer).ToArray();
                    //finally we can load the line from the file
                    var newLines =  file.ReadLine(added, (lineIndex, text) =>
                    {
                        var isEndOfTail = indicies.ChangedReason == LinesChangedReason.Tailed 
                                                    && lineIndex.Line > indicies.TailStartsAt;

                        return new Line(lineIndex, text, isEndOfTail ? DateTime.Now : (DateTime?) null);
                    }, indicies.Encoding).ToArray();

                    return new { NewLines = newLines, OldLines = removedLines };
                })

                 .Where(fn=> fn.NewLines.Length + fn.OldLines.Length > 0)
                .Subscribe(changes =>
                {
                    //update observable list
                    lines.Edit(innerList =>
                    {
                        if (changes.OldLines.Any()) innerList.RemoveMany(changes.OldLines);
                        if (changes.NewLines.Any())  innerList.AddRange(changes.NewLines);
                    });
                });
            _cleanUp = new CompositeDisposable(Lines, lines, aggregator, Disposable.Create(()=>isBusy.OnCompleted()));
        }



        private class CombinedResult
        {
            public ScrollRequest Scroll { get;  }
            public LineMatches MatchedLines { get;  }
            public IIndexCollection Incidies { get;  }

            public CombinedResult(ScrollRequest scroll, LineMatches matchedLines, IIndexCollection incidies)
            {
                Scroll = scroll;
                MatchedLines = matchedLines;
                Incidies = incidies;
            }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
