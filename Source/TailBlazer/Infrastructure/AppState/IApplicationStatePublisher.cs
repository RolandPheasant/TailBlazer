namespace TailBlazer.Infrastructure.AppState;

public interface IApplicationStatePublisher
{
    void Publish(ApplicationState state);
}