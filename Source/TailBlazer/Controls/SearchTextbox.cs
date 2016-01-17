
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;


namespace TailBlazer.Controls
{
    [ContentProperty("Text")]
    public class SearchTextbox : TextBox
    {

        public static readonly DependencyProperty UseRegexProperty = DependencyProperty.Register(
            "UseRegex", typeof (bool), typeof (SearchTextbox), new PropertyMetadata(default(bool)));

        public bool UseRegex
        {
            get { return (bool) GetValue(UseRegexProperty); }
            set { SetValue(UseRegexProperty, value); }
        }


        public static readonly DependencyProperty HintProperty = DependencyProperty.Register(
            "Hint", typeof (string), typeof (SearchTextbox), new PropertyMetadata(default(string)));

        public string Hint
        {
            get { return (string) GetValue(HintProperty); }
            set { SetValue(HintProperty, value); }
        }

        static SearchTextbox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchTextbox), new FrameworkPropertyMetadata(typeof(SearchTextbox)));
        }
    }
}
