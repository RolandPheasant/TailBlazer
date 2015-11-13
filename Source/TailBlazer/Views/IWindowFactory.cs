namespace TailBlazer.Views
{
    public interface IWindowFactory
    {
        MainWindow Create(bool showMenu=false);
    }
}