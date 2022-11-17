namespace TailBlazer.Views.WindowManagement;

public interface IWindowFactory
{
    MainWindow Create(IEnumerable<string> files = null);
}