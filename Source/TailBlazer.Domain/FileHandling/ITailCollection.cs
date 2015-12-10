using System;
using DynamicData;

namespace TailBlazer.Domain.FileHandling
{
    public interface ITailCollection : IDisposable
    {
        IObservableCache<Tailer, string> Tails { get; }

        void Add(string searchText, IObservable<ILineProvider> latest, bool isDefault=false);
        void Remove(string searchText);
    }
}