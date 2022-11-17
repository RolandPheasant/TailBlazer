namespace TailBlazer.Infrastructure.AppState;

public interface IApplicationStateNotifier
{
    IObservable<ApplicationState> StateChanged { get; }
}