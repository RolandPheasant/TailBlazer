using System;


namespace TailBlazer.Domain.Infrastructure
{
    /// <summary>
    /// whipped straight from rx.net
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ImmutableArray<T>
    {
        public static readonly ImmutableArray<T> Empty = new();

        private readonly T[] _data;

        public ImmutableArray()
        {
            _data = Array.Empty<T>();
        }

        public ImmutableArray(T[] data)
        {
            _data = data;
        }

        public T[] Data => _data;

        public ImmutableArray<T> Add(T value)
        {
            var newData = new T[_data.Length + 1];

            Array.Copy(_data, newData, _data.Length);
            newData[_data.Length] = value;

            return new ImmutableArray<T>(newData);
        }


        public ImmutableArray<T> Add(T[] newItems)
        {

            var result = new T[_data.Length + newItems.Length];
            _data.CopyTo(result, 0);
            newItems.CopyTo(result, _data.Length);

            return new ImmutableArray<T>(result);
        }

        public ImmutableArray<T> Remove(T value)
        {
            var i = IndexOf(value);
            if (i < 0)
                return this;

            var length = _data.Length;
            if (length == 1)
                return Empty;

            var newData = new T[length - 1];

            Array.Copy(_data, 0, newData, 0, i);
            Array.Copy(_data, i + 1, newData, i, length - i - 1);

            return new ImmutableArray<T>(newData);
        }

        private int IndexOf(T value)
        {
            for (var i = 0; i < _data.Length; ++i)
                if (Equals(_data[i], value))
                    return i;

            return -1;
        }
    }
}