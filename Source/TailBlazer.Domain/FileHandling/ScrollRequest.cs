namespace TailBlazer.Domain.FileHandling
{
    public class ScrollRequest
    {
        public int NumberOfItems { get;  }
        public int FirstIndex { get;  }
        public ScrollingType Type { get; }
        
        public ScrollRequest(int numberOfItems)
        {
            NumberOfItems = numberOfItems;
            Type = ScrollingType.Tail;
        }
        public ScrollRequest(int numberOfItems, int firstIndex)
        {
            NumberOfItems = numberOfItems;
            FirstIndex = firstIndex;
            Type = ScrollingType.User;
        }

    }
}