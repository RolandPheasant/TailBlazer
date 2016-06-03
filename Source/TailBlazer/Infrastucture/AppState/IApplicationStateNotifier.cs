using System;

namespace TailBlazer.Infrastucture.AppState
{
    public interface IApplicationStateNotifier
    {
        IObservable<ApplicationState> StateChanged { get; }
    }
}
