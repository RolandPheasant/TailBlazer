using System;
using DynamicData;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views.WindowManagement
{
    public interface IWindowsController
    {
        IObservableCache<ViewContainer, Guid> Views { get; }

        void Register(ViewContainer item);
        void Remove(ViewContainer item);
    }
}