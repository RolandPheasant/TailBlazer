using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TailBlazer.Views;

namespace TailBlazer.Infrastucture
{
    public interface IViewFactoryRegister
    {
        void Register<T>()
            where T:IViewModelFactory;
    }
}
