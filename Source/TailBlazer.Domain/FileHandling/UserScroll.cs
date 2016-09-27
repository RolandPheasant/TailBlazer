using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public class UserScroll : IUserScroll
    {
        private readonly IObservable<ILineProvider> _lineProvider;
        private readonly IObservable<ScrollRequest> _scrollRequest;

        public UserScroll([NotNull] IObservable<ILineProvider> lineProvider, [NotNull] IObservable<ScrollRequest> scrollRequest)
        {
            if (lineProvider == null) throw new ArgumentNullException(nameof(lineProvider));
            if (scrollRequest == null) throw new ArgumentNullException(nameof(scrollRequest));
            _lineProvider = lineProvider;
            _scrollRequest = scrollRequest;
        }

        public IObservable<IEnumerable<Line>> Scroll()
        {

            return Observable.Create<IEnumerable<Line>>(observer =>
            {
                var locker = new object();
                var scrollRequest = _scrollRequest.Synchronize(locker);
                var lineProvider = EmptyLineProvider.Instance;

                //TODO: Should I force a read on the first scroll request
                var latest = _lineProvider
                        .Subscribe(lp =>
                        {
                            Interlocked.Exchange(ref lineProvider, lp);
                        });

                var scroller = scrollRequest
                   .Select(request =>
                    {
                        if (request == ScrollRequest.None || request.PageSize == 0 || lineProvider.Count == 0)
                            return new Line[0];

                        return lineProvider.ReadLines(request).ToArray();
                    });

                var notifier = scroller.SubscribeSafe(observer);
                return new CompositeDisposable(latest, notifier);
            });
        }

    }
}