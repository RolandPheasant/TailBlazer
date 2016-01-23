using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TailBlazer.Domain.Infrastructure
{
    public enum PropertyType
    {
        EagerSubscription,
        LazySubscription
    }

    public static class ReactiveEx
    {

        public static IDisposable SetAsComplete<T>(this ISubject<T> source)
        {
            return Disposable.Create(source.OnCompleted);
        }


        public static IProperty<T> ForBinding<T>(this IObservable<T> source, PropertyType type = PropertyType.EagerSubscription)
        {
            return new HungryProperty<T>(source);
        }

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

        public static void Once(this ISubject<Unit> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            source.OnNext(Unit.Default);
        }

        /// <summary>
        /// from here http://haacked.com/archive/2012/10/08/writing-a-continueafter-method-for-rx.aspx/
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TRet"></typeparam>
        /// <param name="observable"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IObservable<TRet> WithContinuation<T, TRet>(
          this IObservable<T> observable, Func<IObservable<TRet>> selector)
        {
            return observable.AsCompletion().SelectMany(_ => selector());
        }

        public static IObservable<Unit> AsCompletion<T>(this IObservable<T> observable)
        {
            return Observable.Create<Unit>(observer =>
            {
                Action onCompleted = () =>
                {
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                };
                return observable.Subscribe(_ => { }, observer.OnError, onCompleted);
            });
        }


    }
}
