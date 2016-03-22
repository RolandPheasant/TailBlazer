using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TailBlazer.Infrastucture;

namespace TailBlazer.Controls
{
    public static class ListboxItemHighlightAssist
    {
        public static readonly DependencyProperty HighlightBackgroundBrushProperty = 
            DependencyProperty.RegisterAttached("HighlightBackgroundBrush", 
                typeof(Brush), 
                typeof(ListboxItemHighlightAssist), 
                new PropertyMetadata(default(Brush), OnPropertyChanged));

        public static void SetHighlightBackgroundBrush(ListBoxItem element, Brush value)
        {
            element.SetValue(HighlightBackgroundBrushProperty, value);
        }
        public static Brush GetHighlightBackgroundBrush(ListBoxItem element)
        {
            return (Brush)element.GetValue(HighlightBackgroundBrushProperty);
        }

        public static readonly DependencyProperty HighlightForegroundBrushProperty =
        DependencyProperty.RegisterAttached("HighlightForegroundBrush",
            typeof(Brush),
            typeof(ListboxItemHighlightAssist),
            new PropertyMetadata(default(Brush), OnPropertyChanged));

        public static void SetHighlightForegroundBrush(ListBoxItem element, Brush value)
        {
            element.SetValue(HighlightForegroundBrushProperty, value);
        }
        public static Brush GetHighlightForegroundBrush(ListBoxItem element)
        {
            return (Brush)element.GetValue(HighlightForegroundBrushProperty);
        }

        public static readonly DependencyProperty IsRecentProperty = 
            DependencyProperty.RegisterAttached("IsRecent", 
                typeof (bool), 
                typeof (ListboxItemHighlightAssist), 
                new PropertyMetadata(default(bool), OnPropertyChanged));

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

            var foreground = GetHighlightForegroundBrush(sender);
            var background = GetHighlightBackgroundBrush(sender);
            if (foreground == null || background == null)
                return;

            //Foreground
            var foregroundAnimation = new ColorAnimation
            {
                From = ((SolidColorBrush)background).Color,
                Duration = new Duration(TimeSpan.FromSeconds(5))
            };
            Storyboard.SetTarget(foregroundAnimation, sender);
            Storyboard.SetTargetProperty(foregroundAnimation, new PropertyPath("(Control.Foreground).Color"));

            var sb = new Storyboard();
          //  sb.Children.Add(backgroundAnimation);
            sb.Children.Add(foregroundAnimation);
            sb.Begin();
        }


    }
}
