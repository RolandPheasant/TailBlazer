using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public class AutoTail: IAutoTail
    {
        private readonly IObservable<ILineProvider> _lineProvider;
        private readonly IObservable<int> _pageSize;

        public AutoTail([NotNull] IObservable<ILineProvider> lineProvider, [NotNull] IObservable<int> pageSize )
        {
            if (lineProvider == null) throw new ArgumentNullException(nameof(lineProvider));
            if (pageSize == null) throw new ArgumentNullException(nameof(pageSize));
            _lineProvider = lineProvider;
            _pageSize = pageSize;
        }

        public IObservable<IEnumerable<Line>> Tail()
        {

            return Observable.Create<IEnumerable<Line>>(observer =>
            {
                var lastTail = TailInfo.None;

                var shared = _lineProvider.Publish();
                //int pageSize = 0;

                var initial = shared.Take(1)
                    .CombineLatest(_pageSize, (lp, pageSize) =>
                    {
                        Interlocked.Exchange(ref lastTail, lp.TailInfo);
                        return lp.ReadLines(new ScrollRequest(pageSize)).ToArray();
                    });

                var subsequent = shared.Skip(1)
                    .DistinctUntilChanged(x=>x.TailInfo.TailStartsAt)
                    .CombineLatest(_pageSize, (lp, pageSize) =>
                    {
                        var result = lp.ReadLines(new ScrollRequest(ScrollReason.TailOnly, pageSize, lastTail.TailStartsAt)).ToArray();
                        Interlocked.Exchange(ref lastTail, lp.TailInfo);
                        return result;
                    });

                var notifier = initial.Concat(subsequent).SubscribeSafe(observer);

                return new CompositeDisposable(shared.Connect(), notifier);
            });
        }
    }
}