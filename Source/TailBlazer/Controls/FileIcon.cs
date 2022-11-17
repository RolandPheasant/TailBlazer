using System.Windows;
using System.Windows.Controls;

namespace TailBlazer.Controls;

public class FileIcon : Control
{
    static FileIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FileIcon), new FrameworkPropertyMetadata(typeof(FileIcon)));
    }
}