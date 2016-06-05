namespace TailBlazer.Infrastucture.AppState
{
    public interface IApplicationStatePublisher
    {
        void Publish(ApplicationState state);
    }
}