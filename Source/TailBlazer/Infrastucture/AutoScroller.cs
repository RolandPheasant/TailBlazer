using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TailBlazer.Infrastucture
{
    public class AutoScroller : IDependencyObjectReceiver
    {
        private ScrollViewer _scrollViewer;

        public void Receive(DependencyObject value)
        {
            _scrollViewer = (ScrollViewer) value;
        }

        public void ScrollToEnd()
        {
            _scrollViewer?.ScrollToEnd();
        }
    }
}
