
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Controls
{

    public enum SearchResultIndicatorStatus
    {
        None,
        Regex,
        Text
    }

    [TemplatePart(Name = TemplateParts.Regex, Type = typeof(RegexMatchedIcon))]
    [TemplatePart(Name = TemplateParts.Text, Type = typeof(TextMatchedIcon))]
    [TemplateVisualState(Name = SearchResultIndicatorStates.None, GroupName = "Indicator")]
    [TemplateVisualState(Name = SearchResultIndicatorStates.Regex, GroupName = "Indicator")]
    [TemplateVisualState(Name = SearchResultIndicatorStates.Text, GroupName = "Indicator")]
    public class SearchResultIndicator : Control
    {
        [UsedImplicitly]
        private class SearchResultIndicatorStates
        {
            public const string None = "None";
            public const string Regex = "Regex";
            public const string Text = "Text";
        }

        [UsedImplicitly]
        private class TemplateParts
        {
            public const string Regex = "PART_RegexImage";
            public const string Text = "PART_TextImage";
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register("Status", typeof (SearchResultIndicatorStatus), typeof (SearchResultIndicator), new PropertyMetadata(SearchResultIndicatorStatus.None, OnStatusPropertyChanged));

        public SearchResultIndicatorStatus Status
        {
            get { return (SearchResultIndicatorStatus) GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }


        //public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
        //    "Foreground", typeof (Brush), typeof (SearchResultIndicator), new PropertyMetadata(default(Brush)));

        //public Brush Foreground
        //{
        //    get { return (Brush) GetValue(ForegroundProperty); }
        //    set { SetValue(ForegroundProperty, value); }
        //}

        static SearchResultIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchResultIndicator), new FrameworkPropertyMetadata(typeof(SearchResultIndicator)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateVisualState(false);
        }

        private static void OnStatusPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var indicator = (SearchResultIndicator)sender;
            var newStatus = (SearchResultIndicatorStatus) args.NewValue;
            var oldStatus = (SearchResultIndicatorStatus)args.OldValue;

            if (newStatus != oldStatus)
                indicator.UpdateVisualState(true);
        }


        private void UpdateVisualState(bool useTransitions)
        {
            switch (this.Status)
            {
                case SearchResultIndicatorStatus.Regex:
                    var x = VisualStateManager.GoToState(this, SearchResultIndicatorStates.Regex, useTransitions);
                    break;
                case SearchResultIndicatorStatus.Text:
                    VisualStateManager.GoToState(this, SearchResultIndicatorStates.Text, useTransitions);
                    break;
                default:
                    VisualStateManager.GoToState(this, SearchResultIndicatorStates.None, useTransitions);
                    break;
            }
        }

        //private void UpdateVisualState(SearchResultIndicatorStatus status)
        //{


        //}
    }
}
