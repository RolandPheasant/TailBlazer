using System.Windows;
using System.Windows.Controls;

namespace TailBlazer.Controls;

public class SettingsIcon : Control
{
    static SettingsIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SettingsIcon), new FrameworkPropertyMetadata(typeof(SettingsIcon)));
    }
}