using TailBlazer.Infrastucture;

namespace TailBlazer.Views
{
    public interface IWindowsController
    {
        void Register(ViewContainer item);
        void Remove(ViewContainer item);
    }
}