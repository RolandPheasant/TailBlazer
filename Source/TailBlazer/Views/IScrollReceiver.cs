namespace TailBlazer.Views
{
    public interface IScrollReceiver
    {
        void ScrollTo(ScrollValues values);
        void ScrollChanged(UserScrollData scrollData);
    }
}