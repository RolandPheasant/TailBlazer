namespace TailBlazer.Infrastucture
{
    public interface IWindowFactory
    {
        MainWindow Create(bool showMenu=false);
    }
}