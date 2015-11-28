using System;

namespace TailBlazer.Domain.Infrastructure
{
    public interface IProperty<out T>: IDisposable
    {
        T Value { get; }
    }
}