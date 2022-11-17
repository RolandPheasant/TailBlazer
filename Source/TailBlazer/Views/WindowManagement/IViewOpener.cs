using TailBlazer.Infrastructure;

namespace TailBlazer.Views.WindowManagement;

public interface IViewOpener
{
    void OpenView(HeaderedView headeredView);
}