using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace TailBlazer.Views.Formatting
{
    public  class TextBlockFormatter
    {
        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached(
            "FormattedText", typeof (IEnumerable<FormattedText>), typeof (TextBlockFormatter), new PropertyMetadata(default(IEnumerable<FormattedText>),OnFormattedTextChanged));


        public static void SetFormattedText(UIElement element, IEnumerable<FormattedText> value)
        {
            element.SetValue(FormattedTextProperty, value);
        }

        public static IEnumerable<FormattedText> GetFormattedText(UIElement element)
        {
            return (IEnumerable<FormattedText>)element.GetValue(FormattedTextProperty);
        }

        public static void OnFormattedTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {

            var textBlock = (TextBlock) sender;
            var textBlocks = (IEnumerable<FormattedText>)args.NewValue;

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
                return new Run(ft.Text);
            }));
        }

    }
}