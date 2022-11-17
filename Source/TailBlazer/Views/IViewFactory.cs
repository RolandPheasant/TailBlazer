using TailBlazer.Domain.Settings;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views;

public interface IViewModelFactory
{
    HeaderedView Create(ViewState state);

    string Key { get; }
}