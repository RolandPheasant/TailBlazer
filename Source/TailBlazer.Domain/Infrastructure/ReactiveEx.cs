using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace TailBlazer.Domain.Infrastructure
{
    public static class ReactiveEx
    {

        public static IObservable<TSource> Previous<TSource>(this IObservable<TSource> source)
        {
            return source.PairWithPrevious().Select(pair => pair.Previous);
        }


        public static IObservable<CurrentAndPrevious<TSource>> PairWithPrevious<TSource>(this IObservable<TSource> source)
        {
            return source.Scan(Tuple.Create(default(TSource), default(TSource)),
                (acc, current) => Tuple.Create(acc.Item2, current))
                .Select(pair=>new CurrentAndPrevious<TSource>(pair.Item1,pair.Item2));
        }

        public class CurrentAndPrevious<T>
        {
            public T Currrent { get;  }
            public T Previous { get;  }

            public CurrentAndPrevious(T currrent, T previous)
            {
                Currrent = currrent;
                Previous = previous;
            }
        }

        public static IObservable<Unit> ToUnit<T>(this IObservable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.Select(_ => Unit.Default);
        }

        public static IObservable<Unit> StartWithUnit(this IObservable<Unit> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.StartWith(Unit.Default);
        }

        public static IObservable<T> ObserveLatestOn<T>(this IObservable<T> source, IScheduler scheduler=null)
        {

            scheduler = scheduler ?? Scheduler.Default;
            return Observable.Create<T>(observer =>
            {
                Notification<T> outsideNotification = null;
                var gate = new object();
                bool active = false;
                var cancelable = new SerialDisposable();
                var disposable = source.Materialize().Subscribe(thisNotification =>
                {
                    bool wasNotAlreadyActive;
                    lock (gate)
                    {
                        wasNotAlreadyActive = !active;
                        active = true;
                        outsideNotification = thisNotification;
                    }

                    if (wasNotAlreadyActive)
                    {
                        cancelable.Disposable = scheduler.Schedule(self =>
                        {
                            Notification<T> localNotification = null;
                            lock (gate)
                            {
                                localNotification = outsideNotification;
                                outsideNotification = null;
                            }
                            localNotification.Accept(observer);
                            bool hasPendingNotification = false;
                            lock (gate)
                            {
                                hasPendingNotification = active = (outsideNotification != null);
                            }
                            if (hasPendingNotification)
                            {
                                self();
                            }
                        });
                    }
                });
                return new CompositeDisposable(disposable, cancelable);
            });
        }

    }
}
