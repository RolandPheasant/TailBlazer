namespace TailBlazer.Domain.Settings
{
    public interface IPersistentView
    {
        ViewState CaptureState();

        void Restore(ViewState state);

    }
}