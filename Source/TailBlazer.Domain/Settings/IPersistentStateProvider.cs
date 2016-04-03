namespace TailBlazer.Domain.Settings
{
    public interface IPersistentStateProvider
    {
        State CaptureState();

        void Restore(State state);

    }
}