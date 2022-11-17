using DynamicData.Kernel;
using TailBlazer.Views;

namespace TailBlazer.Infrastucture;

public interface IViewFactoryProvider
{
    Optional<IViewModelFactory> Lookup(string key);
}