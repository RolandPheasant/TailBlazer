using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TailBlazer.Infrastructure;

public class InvertedBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo language)
    {
        return (value is bool && (bool)value) ? Visibility.Collapsed : Visibility.Visible;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
    {
        return value is Visibility && (Visibility)value == Visibility.Collapsed;
    }
}

public class BooleanToVisibilityConverter : IValueConverter
{
    

    public object Convert(object value, Type targetType, object parameter, CultureInfo language)
    {
        return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
    {
        return value is Visibility && (Visibility)value == Visibility.Visible;
    }
}

public class BooleanToHiddenConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo language)
    {
        return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Hidden;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
    {
        return value is Visibility && (Visibility)value == Visibility.Hidden;
    }
}


public class EqualityToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter,CultureInfo culture)
    {
        return value.Equals(parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter,CultureInfo culture)
    {
        return value.Equals(true) ? parameter : Binding.DoNothing;
    }
}

public class EqualsToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value.Equals(parameter) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}

public class NotEqualsToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !value.Equals(parameter) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}