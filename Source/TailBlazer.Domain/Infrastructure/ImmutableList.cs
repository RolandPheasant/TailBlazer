using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DynamicData;

namespace TailBlazer.Domain.Infrastructure
{
    public class ImmutableList<T>
    {
        public static readonly ImmutableList<T> Empty = new ImmutableList<T>();

        private readonly List<T> _data;
        public IReadOnlyCollection<T> Data => new ReadOnlyCollection<T>(_data);

        public ImmutableList()
        {
            _data = new List<T>();
        }

        public ImmutableList(List<T> data)
        {
            _data = new List<T>(data);
        }
        public ImmutableList(IReadOnlyList<T> data)
        {
            _data = new List<T>(data);
        }

        public ImmutableList(IReadOnlyCollection<T> data)
        {
            _data = new List<T>(data);
        }
        public ImmutableList<T> Add(T value)
        {
            var newData = new List<T>(_data) {value};
            return new ImmutableList<T>(newData);
        }


        public T this[int index]
        {
            get { return _data[index]; }
        }


        public ImmutableList<T> Add(T[] newItems)
        {
            var list = new List<T>(_data);
            list.AddRange(newItems);
            return new ImmutableList<T>(list);
        }

        public ImmutableList<T> Add(IList<T> newItems)
        {
            var list = new List<T>(_data);
            list.AddRange(newItems);
            return new ImmutableList<T>(list);
        }
        public ImmutableList<T> Add(ImmutableList<T> newItems)
        {
            var list = new List<T>(_data);
            list.AddRange(newItems.Data);
            return new ImmutableList<T>(list);
        }


        public ImmutableList<T> Remove(T value)
        {
            var list = new List<T>(_data);
            if (!list.Remove(value))
                return this;

            return new ImmutableList<T>(list);
        }

        public ImmutableList<T> Remove(IEnumerable<T> oldItems)
        {
            var list = new List<T>(_data);
            list.RemoveMany(oldItems);
            return new ImmutableList<T>(list);
        }

        public int Count => _data.Count;

        public int IndexOf(T value)
        {
            return _data.IndexOf(value);
        }
    }
}