using System.Windows;
using System.Windows.Controls;

namespace TailBlazer.Controls
{

    public class RegexMatchedIcon : Control
    {
        static RegexMatchedIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RegexMatchedIcon), new FrameworkPropertyMetadata(typeof(RegexMatchedIcon)));
        }
    }
}
