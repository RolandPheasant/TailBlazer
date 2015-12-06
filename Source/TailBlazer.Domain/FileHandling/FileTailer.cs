using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using DynamicData;
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
            scrollRequest = scrollRequest.Synchronize(locker); ;
            
            var fileWatcher = file.WatchFile(scheduler: scheduler)
                            .DistinctUntilChanged()
                            .TakeWhile(notification => notification.Exists).Repeat()
                            .Replay(1).RefCount();

            var indexer = fileWatcher.Index().Replay(1).RefCount();

            //compare latest lines and latest filter and only take the filtered results it is not empty
            var latestLines = indexer.Cast<ILineProvider>().Synchronize(locker);
            var latestFilter = filter.Cast<ILineProvider>().Synchronize(locker); 
            var latest = latestLines.CombineLatest(latestFilter, (l, f) => f.IsEmpty ? l : f);


            var tailStartsAt = latestLines.Select(p => p.ChangedReason!= LinesChangedReason.Loaded ?  p.TailStartsAt : -1);

            IsLoading = indexer.Take(1).Select(_ => false).StartWith(true);

            //count matching lines (all if no filter is specified)
            MatchedLines = latest.Select(provider => provider.Count);
            TotalLines = latestLines.Select(x => x.Count);

            FileSize = fileWatcher.Select(notification => notification.Size);
            
            var aggregator = latestLines.CombineLatest(latestFilter, (l, f) => f.IsEmpty ? l : f)
                .CombineLatest(scrollRequest, (currentLines, scroll) => currentLines.GetIndicies(scroll).ToArray());


            var xx = tailStartsAt
                .CombineLatest(aggregator, (tail, currentPage ) =>
                {
                    //calculated added and removed indicies
                    var previous = lines.Items.Select(l => l.LineInfo).ToArray();
                    var removed = previous.Except(currentPage, LineInfo.FlexComparer).ToArray();
                    var added = currentPage.Except(previous, LineInfo.FlexComparer).ToArray();

                    //calculated added and removed lines
                    var removedLines = lines.Items.Where(l => removed.Contains(l.LineInfo)).ToArray();

                    Func<long, DateTime?> isTail = l =>
                    {
                        var onTail = tail!=-1 && l >= tail;
                        Console.WriteLine($"Checking {l} os on tail = {tail}/ On tail ={onTail}");
                        return l >= tail ? DateTime.Now : (DateTime?) null;
                    };

                    //finally we can load the line from the file todo: Add encdoing back in
                    var newLines = file.ReadLine(added, (lineIndex, text) => new Line(lineIndex, text, isTail(lineIndex.Start)), Encoding.UTF8).ToArray();
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
            _cleanUp = new CompositeDisposable(Lines, lines, xx, Disposable.Create(() => isBusy.OnCompleted()));
        }


        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
