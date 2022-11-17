using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TailBlazer.Infrastucture;

public class SolidColorAnimation : ColorAnimation
{
    public SolidColorBrush ToBrush
    {
        get => To == null ? null : new SolidColorBrush(To.Value);
        set => To = value?.Color;
    }

    public SolidColorBrush FromBrush
    {
        get => From == null ? null : new SolidColorBrush(From.Value);
        set => From = value?.Color;
    }
}