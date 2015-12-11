using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    public class LineScroller : ILineScroller
    {
        private readonly IDisposable _cleanUp;
        
        public IObservableList<Line> Lines { get; }


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

            var lines = new SourceList<Line>();
            Lines = lines.AsObservableList();

            var locker = new object();
            scrollRequest = scrollRequest.Synchronize(locker);


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


        public LineScroller([NotNull] IObservable<ILineProvider> latest, [NotNull] IObservable<ScrollRequest> scrollRequest)
        {
            if (latest == null) throw new ArgumentNullException(nameof(latest));
            if (scrollRequest == null) throw new ArgumentNullException(nameof(scrollRequest));

            var lines = new SourceList<Line>();
            Lines = lines.AsObservableList();

            var locker = new object();

            scrollRequest = scrollRequest.Synchronize(locker);
            latest = latest.Synchronize(locker);

            var aggregator = latest.CombineLatest(scrollRequest, (currentLines, scroll) => currentLines.ReadLines(scroll).ToArray())
                .Subscribe(currentPage =>
                {
                    var previous = lines.Items.ToArray();
                    var added = currentPage.Except(previous, Line.TextStartComparer).ToArray();
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
