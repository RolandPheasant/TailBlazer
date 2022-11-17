using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Controls;

public static class TextBlockFormatter
{
    public static readonly DependencyProperty ForegroundProperty = DependencyProperty.RegisterAttached("Foreground", typeof (Brush), typeof (TextBlockFormatter), new PropertyMetadata(default(Brush)));


    public static void SetForeground(UIElement element, Brush value)
    {
        element.SetValue(ForegroundProperty, value);
    }

    public static Brush GetForeground(UIElement element)
    {
        return (Brush)element.GetValue(ForegroundProperty);
    }



    public static readonly DependencyProperty HighlightBackgroundProperty = DependencyProperty.RegisterAttached("HighlightBackground", typeof (Brush), typeof (TextBlockFormatter), new PropertyMetadata(default(Brush), OnBackgroundChanged));

    public static void SetHighlightBackground(UIElement element, Brush value)
    {
        element.SetValue(HighlightBackgroundProperty, value);
    }

    public static Brush GetHighlightBackground(UIElement element)
    {
        return (Brush)element.GetValue(HighlightBackgroundProperty);
    }

    public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached("FormattedText", typeof (IEnumerable<DisplayText>), typeof (TextBlockFormatter), new PropertyMetadata(default(IEnumerable<DisplayText>),OnFormattedTextChanged));





    public static void SetFormattedText(UIElement element, IEnumerable<DisplayText> value)
    {
        element.SetValue(FormattedTextProperty, value);
    }

    public static IEnumerable<DisplayText> GetFormattedText(UIElement element)
    {
        return (IEnumerable<DisplayText>)element.GetValue(FormattedTextProperty);
    }

    public static void OnBackgroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {

        //if (e)
    }

    public static void OnFormattedTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {

        var textBlock = (TextBlock) sender;
        var textBlocks = (IEnumerable<DisplayText>)args.NewValue;

        //Run run = new Run(searchedString)
        //{
        //Background = isHighlight ? this.HighlightBackground : this.Background,
        //Foreground = isHighlight ? this.HighlightForeground : this.Foreground,

        // Set the source text with the style which is Italic.
        //   FontStyle = isHighlight ? FontStyles.Italic : FontStyles.Normal,

        // Set the source text with the style which is Bold.
        //FontWeight = isHighlight ? FontWeights.Bold : FontWeights.Normal
        //};

        textBlock.Inlines.Clear();
        textBlock.Inlines.AddRange(textBlocks.Select(ft =>
        {
            var run = new Run(ft.Text);

            var background = GetHighlightBackground(textBlock);
            if (background != null) run.Background = background;

            return new Run(ft.Text);
        }));
    }

}