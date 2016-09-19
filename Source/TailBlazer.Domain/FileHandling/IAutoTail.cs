using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace TailBlazer.Domain.FileHandling
{
    public interface IAutoTail
    {
        IObservable<IEnumerable<Line>> Tail(int pageSize);
    }

    public class AutoTail: IAutoTail
    {
        private readonly IObservable<ILineProvider> _lineProvider;

        public AutoTail(IObservable<ILineProvider> lineProvider)
        {
            _lineProvider = lineProvider;
        }

        public IObservable<IEnumerable<Line>> Tail(int pageSize)
        {
            if (pageSize <= 1) throw new ArgumentOutOfRangeException(nameof(pageSize));

            return Observable.Create<IEnumerable<Line>>(observer =>
            {
                var lastTail = TailInfo.None;

                var shared = _lineProvider.Publish();

                var initial = shared.Take(1).Select(lp =>
                {
                    Interlocked.Exchange(ref lastTail, lp.TailInfo);
                    return lp.ReadLines(new ScrollRequest(pageSize)).ToArray();
                });

                var subsequent = shared.Skip(1)
                    .DistinctUntilChanged(x=>x.TailInfo.TailStartsAt)
                    .Select(lp =>
                    {
                        var newLines = lp.ReadLines(new ScrollRequest(ScrollReason.TailOnly, pageSize,  lastTail.TailStartsAt)).ToArray();
                        Interlocked.Exchange(ref lastTail, lp.TailInfo);
                        return newLines.ToArray();
                    });

                var notifier = initial.Concat(subsequent).SubscribeSafe(observer);

                return new CompositeDisposable(shared.Connect(), notifier);
            });
        }
    }
}