using System;
using System.Collections.Generic;
using DynamicData.Binding;

namespace TailBlazer.Domain.Infrastructure
{


    public  static class Equality
    {
        public static IEqualityComparer<T> CompareOn<T, TValue>(Func<T, TValue> valueSelector)
        {
            return new GenericEqualityComparer<T>((t1, t2) => EqualityComparer<TValue>.Default.Equals(valueSelector(t1), valueSelector(t2)),
                    t => EqualityComparer<TValue>.Default.GetHashCode(valueSelector(t)));
        }

        public static IEqualityComparer<T> AndOn<T, TValue>(this IEqualityComparer<T> rootComparer, Func<T, TValue> valueSelector)
        {
            return new GenericEqualityComparer<T>(rootComparer, (t1, t2) => EqualityComparer<TValue>.Default.Equals(valueSelector(t1), valueSelector(t2)),
                    t => EqualityComparer<TValue>.Default.GetHashCode(valueSelector(t)));
        }


        public static IEqualityComparer<T> Create<T,TValue>(Func<T, TValue> projection)
        {
            return new GenericEqualityComparer<T>((t1, t2) => EqualityComparer<TValue>.Default.Equals(projection(t1), projection(t2)),
                t => EqualityComparer<TValue>.Default.GetHashCode(projection(t)));
        }

    }

 
    public class GenericEqualityComparer<T> : IEqualityComparer<T>
    {
        readonly Func<T, T, bool> _compareFunction;
        readonly Func<T, int> _hashFunction;

        public GenericEqualityComparer(IEqualityComparer<T> rootComparer,Func<T, T, bool> compareFunction, Func<T, int> hashFunction)
        {

            _hashFunction = (t) =>
            {
                unchecked
                {
                    var hashCode = rootComparer.GetHashCode(t);
                    hashCode = (hashCode*397) ^ hashFunction(t);
                    return hashCode;
                }
            };

            _compareFunction = (x, y) => rootComparer.Equals(x, y) && compareFunction(x, y);
        }

        public GenericEqualityComparer(Func<T, T, bool> compareFunction, Func<T, int> hashFunction)
        {
            _compareFunction = compareFunction;
            _hashFunction = hashFunction;
        }

        public bool Equals(T x, T y)
        {
            return _compareFunction(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _hashFunction(obj);
        }
    }

}
