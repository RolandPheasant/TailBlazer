using TailBlazer.Domain.Settings;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views
{

    public interface IViewModelFactory
    {
        ViewContainer Create(ViewState state);

        string Key { get; }
    }
}
