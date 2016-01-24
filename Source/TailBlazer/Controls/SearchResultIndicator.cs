
using System.Windows;
using System.Windows.Controls;

namespace TailBlazer.Controls
{

    public enum SearchResultIndicatorStatus
    {
        None,
        Regex,
        Filter
    }

    public class SearchResultIndicator : Control
    {
        static SearchResultIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchResultIndicator), new FrameworkPropertyMetadata(typeof(SearchResultIndicator)));
        }
    }
}
