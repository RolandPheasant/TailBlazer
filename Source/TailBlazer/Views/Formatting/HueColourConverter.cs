using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Views.Formatting
{

    public enum HueBrushTarget
    {
        Foreground,
        Background
    }

    public class HueConverter :  IMultiValueConverter
    {

        public HueBrushTarget Target { get; set; }
        

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var hue = (Hue)values[0];
            var defaultColour = (Brush)values[1];

            if (hue != Hue.NotSpecified)
                return Target == HueBrushTarget.Foreground ? hue.ForegroundBrush : defaultColour;

            return defaultColour;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}