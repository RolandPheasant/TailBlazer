using System.Windows.Controls.Primitives;

namespace TailBlazer.Views
{
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
}