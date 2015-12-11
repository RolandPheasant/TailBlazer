using System;
using DynamicData;

namespace TailBlazer.Domain.FileHandling
{
    public interface ILineScroller : IDisposable
    {
        IObservableList<Line> Lines { get; }
    }
}