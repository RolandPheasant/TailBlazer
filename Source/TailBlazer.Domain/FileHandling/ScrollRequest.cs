namespace TailBlazer.Domain.FileHandling
{
    public class ScrollRequest
    {
        public int PageSize { get;  }
        public int FirstIndex { get;  }
        public ScrollingMode Mode { get; }
        
        public ScrollRequest(int pageSize)
        {
            PageSize = pageSize;
            Mode = ScrollingMode.Tail;
        }
        public ScrollRequest(int pageSize, int firstIndex)
        {
            PageSize = pageSize;
            FirstIndex = firstIndex;
            Mode = ScrollingMode.User;
        }

    }
}