
using System.Windows;

namespace TailBlazer.Views.Layout
{

    public class ShellState
    {
        public double Top { get; }
        public double Left { get; }

        public double Width { get; }

        public double Height { get; }

        public WindowState State { get; }

        public ShellState(double top, double left, double width, double height, WindowState state)
        {
            Top = top;
            Left = left;
            Width = width;
            Height = height;
            State = state;
        }
    }
}
