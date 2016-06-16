namespace TailBlazer.KeyboardNavigation
{
    public interface IPageProvider
    {
        int PageSize { get; }
        int FirstIndex { get; }
    }
}
