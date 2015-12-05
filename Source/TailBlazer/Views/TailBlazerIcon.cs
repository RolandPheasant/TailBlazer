using System.Windows;
using System.Windows.Controls;

namespace TailBlazer.Views
{
    public class TailBlazerIcon : Control
    {
        static TailBlazerIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TailBlazerIcon), new FrameworkPropertyMetadata(typeof(TailBlazerIcon)));
        }
    }
}
