using System.Collections.Generic;

namespace TailBlazer.Views
{
    public interface IWindowFactory
    {
        MainWindow Create(IEnumerable<string> files = null);
    }
}