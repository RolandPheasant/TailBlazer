
using TailBlazer.Views;

namespace TailBlazer.Infrastructure;

public interface IViewFactoryRegister
{
    void Register<T>()
        where T:IViewModelFactory;
}