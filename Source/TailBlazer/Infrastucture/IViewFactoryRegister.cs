
using TailBlazer.Views;

namespace TailBlazer.Infrastucture;

public interface IViewFactoryRegister
{
    void Register<T>()
        where T:IViewModelFactory;
}