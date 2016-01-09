using TailBlazer.Controls;
using TailBlazer.Views;

namespace TailBlazer.Infrastucture
{
    public interface IScrollReceiver
    {
        void ScrollBoundsChanged(ScrollBoundsArgs boundsArgs);
        void ScrollChanged(ScrollChangedArgs scrollChangedArgs);
    }
}