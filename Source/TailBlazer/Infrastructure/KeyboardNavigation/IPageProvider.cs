namespace TailBlazer.Infrastructure.KeyboardNavigation;

public interface IPageProvider
{
    int PageSize { get; }
    int FirstIndex { get; }
}