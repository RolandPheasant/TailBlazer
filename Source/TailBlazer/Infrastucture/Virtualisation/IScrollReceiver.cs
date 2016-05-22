namespace TailBlazer.Infrastucture.Virtualisation
{
    public interface IScrollReceiver
    {
        void ScrollBoundsChanged(ScrollBoundsArgs boundsArgs);
        void ScrollChanged(ScrollChangedArgs scrollChangedArgs);

        void ScrollDiff(int lineChanged);
    }


}