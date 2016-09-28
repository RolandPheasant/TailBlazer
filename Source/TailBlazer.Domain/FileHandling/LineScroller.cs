using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Kernel;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    public class LineScroller : ILineScroller
    {
        private readonly IDisposable _cleanUp;
        
        public IObservableCache<Line,LineKey> Lines { get; }


        [Obsolete("USE OTHER OVERLOAD")]
        public LineScroller(FileInfo file,
                IObservable<ILineProvider> latest,
                IObservable<ScrollRequest> scrollRequest,
                ILogger logger,
                IScheduler scheduler = null)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (latest == null) throw new ArgumentNullException(nameof(latest));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.Info($"Constructing file tailer for {file.FullName}");

            var lines = new SourceCache<Line, LineKey>(l=>l.Key);
            Lines = lines.AsObservableCache();

            var locker = new object();
            scrollRequest = scrollRequest.Synchronize(locker);


            var aggregator = latest.CombineLatest(scrollRequest, (currentLines, scroll) => currentLines.ReadLines(scroll).ToArray())
                .RetryWithBackOff<Line[], Exception>((ex,i)=>TimeSpan.FromSeconds(1))
                .Subscribe(currentPage =>
                {
                    var previous = lines.Items.ToArray();
                    var added = currentPage.Except(previous,Line.TextStartComparer).ToArray();
                    var removed = previous.Except(currentPage, Line.TextStartComparer).ToArray();

                    lines.Edit(innerCache =>
                    {
                        if (removed.Any()) innerCache.Remove(removed);
                        if (added.Any()) innerCache.AddOrUpdate(added);
                    });
                });

            _cleanUp = new CompositeDisposable(Lines, lines, aggregator);
        }


        public LineScroller([NotNull] IObservable<ILineProvider> latest, [NotNull] IObservable<ScrollRequest> scrollRequest)
        {
            if (latest == null) throw new ArgumentNullException(nameof(latest));
            if (scrollRequest == null) throw new ArgumentNullException(nameof(scrollRequest));


            var lines = new SourceCache<Line, LineKey>(l => l.Key);
            Lines = lines.Connect().IgnoreUpdateWhen((current,previous)=> current.Key==previous.Key).AsObservableCache();

            var locker = new object();

            scrollRequest = scrollRequest.Synchronize(locker);
            latest = latest.Synchronize(locker);

            var shared = latest.Synchronize(locker).Publish();
            var isTailing = scrollRequest.Select(request => request.Mode == ScrollReason.Tail).DistinctUntilChanged();
            var tailer = shared.Tail(scrollRequest.Select(request => request.PageSize).DistinctUntilChanged());
            var scroller = shared.Scroll(scrollRequest).DistinctUntilChanged();

            var aggregator = tailer
                .RetryWithBackOff<AutoTailResponse, Exception>((ex, i) => TimeSpan.FromSeconds(1))
                .Subscribe(tail =>
                {
                    lines.Edit(innerCache =>
                    {
                        if (tail.Reason == AutoTailReason.LoadTail) innerCache.Clear();
                        
                        //add new lines
                        innerCache.AddOrUpdate(tail.Lines);

                        if (tail.PageSize >= innerCache.Count) return;
                         
                        //remove unneeded lines
                        var toRemove = innerCache.Items
                                .OrderBy(l => l.LineInfo.Start)
                                .Take(innerCache.Count - tail.PageSize)
                                .Select(line => line.Key)
                                .ToArray();

                        innerCache.Remove(toRemove);
                    });
                });

            _cleanUp = new CompositeDisposable(Lines, lines, aggregator, shared.Connect());
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
