
using System.Windows;
using System.Windows.Controls;

namespace TailBlazer.Controls;

public class SearchIcon : Control
{
    static SearchIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchIcon), new FrameworkPropertyMetadata(typeof(SearchIcon)));
    }
}