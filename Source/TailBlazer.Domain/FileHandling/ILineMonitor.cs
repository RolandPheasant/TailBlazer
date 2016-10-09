using System;
using DynamicData;

namespace TailBlazer.Domain.FileHandling
{
    public interface ILineMonitor: IDisposable
    {
        IObservableCache<Line, LineKey> Lines { get; }
        IObservable<int> TotalLines { get; }
    }

}