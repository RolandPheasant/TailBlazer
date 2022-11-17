using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TailBlazer.Infrastructure.AppState;

class ApplicationStateBroker : IApplicationStateNotifier, IApplicationStatePublisher
{
    private readonly ISubject<ApplicationState> _stateChanged = new ReplaySubject<ApplicationState>(1);

    public IObservable<ApplicationState> StateChanged => _stateChanged.DistinctUntilChanged();

    public void Publish(ApplicationState state)
    {
        _stateChanged.OnNext(state);
    }
}