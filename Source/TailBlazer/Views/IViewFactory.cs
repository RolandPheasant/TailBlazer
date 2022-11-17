using TailBlazer.Domain.Settings;
using TailBlazer.Infrastructure;

namespace TailBlazer.Views;

public interface IViewModelFactory
{
    HeaderedView Create(ViewState state);

    string Key { get; }
}