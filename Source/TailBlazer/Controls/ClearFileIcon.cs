using System.Windows;
using System.Windows.Controls;

namespace TailBlazer.Controls
{

    public class ClearFileIcon : Control
    {
        static ClearFileIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ClearFileIcon), new FrameworkPropertyMetadata(typeof(ClearFileIcon)));
        }
    }
}
