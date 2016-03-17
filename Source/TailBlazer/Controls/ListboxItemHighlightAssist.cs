using System;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TailBlazer.Controls
{
    public static class ListboxItemHighlightAssist
    {
        public static readonly DependencyProperty FromBrushProperty = 
            DependencyProperty.RegisterAttached("FromBrush", 
                typeof(Brush), 
                typeof(ListboxItemHighlightAssist), 
                new PropertyMetadata(default(Brush), OnPropertyChanged));

        public static void SetFromBrush(ListBoxItem element, Brush value)
        {
            element.SetValue(FromBrushProperty, value);
        }
        public static Brush GetFromBrush(ListBoxItem element)
        {
            return (Brush)element.GetValue(FromBrushProperty);
        }

        public static readonly DependencyProperty ToBrushProperty =
        DependencyProperty.RegisterAttached("ToBrush",
            typeof(Brush),
            typeof(ListboxItemHighlightAssist),
            new PropertyMetadata(default(Brush), OnPropertyChanged));

        public static void SetToBrush(ListBoxItem element, Brush value)
        {
            element.SetValue(ToBrushProperty, value);
        }
        public static Brush GetToBrush(ListBoxItem element)
        {
            return (Brush)element.GetValue(ToBrushProperty);
        }

        public static readonly DependencyProperty IsRecentProperty = 
            DependencyProperty.RegisterAttached("IsRecent", 
                typeof (bool), 
                typeof (ListboxItemHighlightAssist), 
                new PropertyMetadata(default(bool)));

        public static void SetIsRecent(ListBoxItem element, bool value)
        {
            element.SetValue(IsRecentProperty, value);
        }
        public static bool GetIsRecent(ListBoxItem element)
        {
            return (bool)element.GetValue(IsRecentProperty);
        }


        public static readonly DependencyProperty IsEnabledProperty = 
            DependencyProperty.RegisterAttached("IsEnabled",
            typeof(bool), 
            typeof(ListboxItemHighlightAssist), 
            new PropertyMetadata(default(bool)));
 
        public static void SetIsEnabled(ListBoxItem element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(ListBoxItem element)
        {
            return (bool)element.GetValue(IsEnabledProperty);
        }

        public static readonly DependencyProperty BaseStyleProperty = DependencyProperty.RegisterAttached(
            "BaseStyle", 
            typeof (Style), 
            typeof (ListboxItemHighlightAssist), new PropertyMetadata(default(Style)));

        public static void SetBaseStyle(ListBoxItem element, Style value)
        {
            element.SetValue(BaseStyleProperty, value);
        }

        public static Style GetBaseStyle(ListBoxItem element)
        {
            return (Style)element.GetValue(BaseStyleProperty);
        }

        private static void OnPropertyChanged(DependencyObject dependencyObject,DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var sender = dependencyObject as ListBoxItem;
            if (sender == null) return;

            var enabled = GetIsEnabled(sender);
            var isRecent = GetIsRecent(sender);
            if (!enabled || !isRecent)
                return;

            var animation = new ColorAnimation
            {
                From = ((SolidColorBrush) GetFromBrush(sender)).Color,
                Duration = new Duration(TimeSpan.FromSeconds(5))
            };

            Storyboard.SetTarget(animation, sender);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Control.Foreground).Color"));

            var sb = new Storyboard();
            sb.Children.Add(animation);
            sb.Begin();
        }


    }
}
