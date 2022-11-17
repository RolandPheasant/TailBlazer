namespace TailBlazer.Infrastructure.Virtualisation;

public class ScrollBoundsArgs
{
    public int PageSize { get; }

    public int FirstIndex { get; }

    public ScrollBoundsArgs(int pageSize, int firstIndex)
    {
        PageSize = pageSize;
        FirstIndex = firstIndex;
    }
}