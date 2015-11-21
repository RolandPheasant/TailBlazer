namespace TailBlazer.Views
{
    public interface IScrollReceiver
    {
        void ScrollBoundsChanged(ScrollBoundsArgs boundsArgs);
        void ScrollChanged(ScrollChangedArgs scrollChangedArgs);
    }
}