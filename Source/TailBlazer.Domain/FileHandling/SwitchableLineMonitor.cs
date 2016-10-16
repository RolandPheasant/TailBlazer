using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public sealed class SwitchableLineMonitor:  ILineMonitor
    {
        public IObservableCache<Line, LineKey> Lines { get; }
        public IObservable<int> TotalLines { get; }
        public IObservable<IProgressInfo> Progress { get; }

        private readonly IDisposable _cleanUp;

        public SwitchableLineMonitor([NotNull] IObservable<ILineMonitor> lineProviderObs)
        {
            if (lineProviderObs == null) throw new ArgumentNullException(nameof(lineProviderObs));

            var cache = new SourceCache<Line, LineKey>(l => l.Key);

            var shared = lineProviderObs.Publish();
            var switchableLoader = shared.Select(monitor =>
                {
                    return monitor.Lines
                        .Connect()
                        .FinallySafe(() => cache.Clear());
                }).Switch()
                .PopulateInto(cache);

            TotalLines = shared.Select(monitor => monitor.TotalLines).Switch();
            Progress = shared.Select(monitor => monitor.Progress).Switch();
            Lines = cache.AsObservableCache();

            _cleanUp = new CompositeDisposable(shared.Connect(), cache, switchableLoader, Lines);

        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}