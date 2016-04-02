namespace TailBlazer.Domain.FileHandling
{
    public class Page
    {
        public int Start { get;  }
        public int Size { get;  }

        public Page(int start, int size)
        {
            Start = start;
            Size = size;
        }
    }
}