namespace TailBlazer.Views.Tail
{
    public interface IPageProvider
    {
        int PageSize { get;  }
        int FirstIndex { get;  }
    }
}