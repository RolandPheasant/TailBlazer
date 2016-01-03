using System;
using DynamicData;

namespace TailBlazer.Domain.FileHandling
{
    public interface ILineScroller : IDisposable
    {
        IObservableCache<Line, LineKey> Lines { get; }
    }

}