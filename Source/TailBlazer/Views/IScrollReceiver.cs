namespace TailBlazer.Views
{
    public interface IScrollReceiver
    {
        void ScrollTo(ScrollBoundsArgs boundsArgs);
        void ScrollChanged(ScrollChangedArgs scrollChangedArgs);
    }
}