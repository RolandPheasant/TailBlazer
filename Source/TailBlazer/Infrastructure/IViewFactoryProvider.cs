using DynamicData.Kernel;
using TailBlazer.Views;

namespace TailBlazer.Infrastructure;

public interface IViewFactoryProvider
{
    Optional<IViewModelFactory> Lookup(string key);
}