using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using static System.Reactive.Linq.Observable;

namespace TailBlazer.Domain.Infrastructure
{
    public enum PropertyType
    {
        EagerSubscription,
        LazySubscription
    }

    public static class ReactiveEx
    {

        public static IObservable<T> MakeResettable<T>(this IObservable<T> source)
        {
            var _resetter = new Subject<Unit>();
            return _resetter
                .StartWith(Unit.Default)
                .Select(_ => Defer(() => source))
                .Switch()
                .Publish().RefCount();
        }

        public static IObservable<T> MaxSequenceOfSource<T, TKey>(this IObservable<T> source, Func<T, TKey> selector)
            where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            return Create<T>(observer =>
            {
                var lastKey = default(TKey);
                var largestT = default(T);
                int counter = 1;
                int countOfTailedFile = 0;
                return source
                    .GroupBy(t => t)
                    .Select(t =>
                    {
                        countOfTailedFile++;
                        return new { t.Key, Counter = countOfTailedFile };
                    })
                    .Where(t => t != null)
                    .Subscribe(obj =>
                    {
                        var key = selector(obj.Key);
                        if ((key.CompareTo(lastKey)) == 1)
                        {
                            largestT = obj.Key;
                        }

                        if (obj.Counter == counter)
                        {
                            observer.OnNext(largestT);
                        }

                        counter++;
                        lastKey = key;

                    });
            });

        }

        public static IObservable<TSource> Sort<TSource, TKey>(this IObservable<TSource> source,
            Func<TSource, TKey> keySelector,
            TKey firstKey,
            Func<TKey, TKey> nextKeyFunc)
        {
            return Create<TSource>(o =>
            {
                var nextKey = firstKey;
                var buffer = new Dictionary<TKey, TSource>();
                return source.Subscribe(i =>
                {
                    if (keySelector(i).Equals(nextKey))
                    {
                        nextKey = nextKeyFunc(nextKey);
                        o.OnNext(i);
                        TSource nextValue;
                        while (buffer.TryGetValue(nextKey, out nextValue))
                        {
                            buffer.Remove(nextKey);
                            o.OnNext(nextValue);
                            nextKey = nextKeyFunc(nextKey);
                        }
                    }
                    else
                    {
                        var key = keySelector(i);
                        if (!buffer.ContainsKey(key))
                        {
                            buffer.Add(key, i);
                        }
                    }
                });
            });
        }

        public static IObservable<TSource> OrderedCollectUsingMerge<TSource, TKey>(this IObservable<TSource> left,
            IObservable<TSource> right,
            Func<TSource, TKey> keySelector,
            TKey firstKey,
            Func<TKey, TKey> nextKeyFunc,
            Func<TSource, TSource, TSource> resultSelector)
        {
            Func<IObservable<TSource>, IObservable<TSource>> curriedSort =
                events => events.Sort(keySelector, firstKey, nextKeyFunc);

            return curriedSort(left).Zip(curriedSort(right), resultSelector);
        }

        public static IObservable<TSource> OrderedCollectUsingZip<TSource, TKey>(this IObservable<TSource> left,
            IObservable<TSource> right,
            Func<TSource, TKey> keySelector,
            TKey firstKey,
            Func<TKey, TKey> nextKeyFunc,
            Func<TSource, TSource, TSource> resultSelector)
        {
            return left.Sort(keySelector, firstKey, nextKeyFunc).Zip(right.Sort(keySelector, firstKey, nextKeyFunc),
                resultSelector);
        }

        public static IDisposable SetAsComplete<T>(this ISubject<T> source)
        {
            return Disposable.Create(source.OnCompleted);
        }

        public static IProperty<T> ForBinding<T>(this IObservable<T> source,
            PropertyType type = PropertyType.EagerSubscription)
        {
            return new HungryProperty<T>(source);
        }

        public static IObservable<TSource> Previous<TSource>(this IObservable<TSource> source)
        {
            return source.PairWithPrevious().Select(pair => pair.Previous);
        }

        public static IObservable<CurrentAndPrevious<TSource>> PairWithPrevious<TSource>(
            this IObservable<TSource> source)
        {
            return source.Scan(Tuple.Create(default(TSource), default(TSource)),
                (acc, current) => Tuple.Create(acc.Item2, current))
                .Select(pair => new CurrentAndPrevious<TSource>(pair.Item1, pair.Item2));
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
        ///     from here http://haacked.com/archive/2012/10/08/writing-a-continueafter-method-for-rx.aspx/
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
            return Create<Unit>(observer =>
            {
                Action onCompleted = () =>
                {
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                };
                return observable.Subscribe(_ => { }, observer.OnError, onCompleted);
            });
        }

        public class CurrentAndPrevious<T>
        {
            public CurrentAndPrevious(T currrent, T previous)
            {
                Currrent = currrent;
                Previous = previous;
            }

            public T Currrent { get; }
            public T Previous { get; }
        }
    }
}