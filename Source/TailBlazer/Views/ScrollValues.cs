using System.Windows.Controls.Primitives;

namespace TailBlazer.Views
{
    public class ScrollValues
    {
        public int PageSize { get; }

        public int FirstIndex { get; }

        public IScrollInfo ScrollInfo { get;  }

        public ScrollChangeReason Reason { get;  }

        public ScrollValues(int pageSize, int firstIndex, IScrollInfo scrollInfo, ScrollChangeReason reason= ScrollChangeReason.Automatic)
        {
            PageSize = pageSize;
            FirstIndex = firstIndex;
            ScrollInfo = scrollInfo;
            Reason = reason;
        }
    }
}