using System.Collections.Generic;

namespace TailBlazer.Views.WindowsCreation
{
    public interface IWindowFactory
    {
        MainWindow Create(IEnumerable<string> files = null);
    }
}