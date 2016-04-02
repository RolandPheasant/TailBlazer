using System;

namespace TailBlazer.Domain.Settings
{
    public interface ISetting<T>
    {
        IObservable<T> Value { get; }
         

        void Write(T item);
    }
}