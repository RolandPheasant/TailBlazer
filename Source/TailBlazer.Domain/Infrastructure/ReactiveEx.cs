using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TailBlazer.Domain.Infrastructure
{
    public static class ReactiveEx
    {

        public static IObservable<TSource> Previous<TSource>(this IObservable<TSource> source)
        {
            return source.PairWithPrevious().Select(pair => pair.Previous);
        }


        public static IObservable<CurrentAndPrevious<TSource>>PairWithPrevious<TSource>(this IObservable<TSource> source)
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
    }
}
