using System.Windows.Controls;

namespace TailBlazer.Infrastucture
{
    public interface ISelectionMonitor
    {
        void Receive(ListBox selector);
    }
}