using System;

namespace TailBlazer.Infrastucture.Virtualisation2
{
    public interface IVirtualController<T>
    {
        T Get(int index);

        int Count();

        int IndexOf(T item);

        IObservable<ItemWithIndex<T>[]> ItemsAdded { get; }

        IObservable<int> CountChanged { get; }
    }
}