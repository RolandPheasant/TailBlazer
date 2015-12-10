using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;

using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    public class FileTailer : IDisposable
    {
        private readonly IDisposable _cleanUp;

        //public IObservable<int> MatchedLines { get; }
        public IObservable<long> FileSize { get; }
        public IObservableList<Line> Lines { get; }

        public IObservable<bool> IsLoading { get; }
        public IObservable<ILineProvider> Latest { get; }

        public FileTailer(FileInfo file,
                IObservable<ILineProvider> latest,
                IObservable<ScrollRequest> scrollRequest,
                ILogger logger,
                IScheduler scheduler = null)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (latest == null) throw new ArgumentNullException(nameof(latest));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.Info($"Constructing file tailer for {file.FullName}");

            var lines = new SourceList<Line>();
            Lines = lines.AsObservableList();

            var locker = new object();
            scrollRequest = scrollRequest.Synchronize(locker);

            var fileWatcher = file.WatchFile(scheduler: scheduler)
                            .DistinctUntilChanged()
                            .TakeWhile(notification => notification.Exists).Repeat()
                            .Replay(1).RefCount();

            var indexer = fileWatcher.Index().Replay(1).RefCount();


            Latest = latest;
            FileSize = fileWatcher.Select(notification => notification.Size);
            IsLoading = indexer.Take(1).Select(_ => false).StartWith(true);

            var aggregator = latest.CombineLatest(scrollRequest, (currentLines, scroll) => currentLines.ReadLines(scroll).ToArray())
                .Subscribe(currentPage =>
                {
                    var previous = lines.Items.ToArray();
                    var added = currentPage.Except(previous,Line.TextStartComparer).ToArray();
                    var removed = previous.Except(currentPage, Line.TextStartComparer).ToArray();

                    lines.Edit(innerList =>
                    {
                        if (removed.Any()) innerList.RemoveMany(removed);
                        if (added.Any()) innerList.AddRange(added);
                    });
                });

            _cleanUp = new CompositeDisposable(Lines, lines, aggregator);
        }
        
        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
