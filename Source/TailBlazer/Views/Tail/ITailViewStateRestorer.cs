using TailBlazer.Domain.Settings;

namespace TailBlazer.Views.Tail
{
    public interface ITailViewStateRestorer
    {
        void Restore(TailViewModel view, State state);
        void Restore(TailViewModel view, TailViewState tailviewstate);
    }
}