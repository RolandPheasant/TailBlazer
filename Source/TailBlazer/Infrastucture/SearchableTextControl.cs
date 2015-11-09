/********************************** Module Header **********************************\
* Module Name:  SearchableTextControl.cs
* Project:      CSWPFSearchAndHighlightTextBlockControl
* Copyright (c) Microsoft Corporation.
*
* The SearchableTextControl.cs file defines a User Control Class in order to search for
* keyword and highlight it when the operation gets the result.
*
*
* This source is subject to the Microsoft Public License.
* See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
* All other rights reserved.
*
* THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
* EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
* MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***********************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace TailBlazer.Infrastucture
{
    public class SearchableTextControl : Control
    {
        static SearchableTextControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchableTextControl),
                new FrameworkPropertyMetadata(typeof(SearchableTextControl)));
        }

        #region DependencyProperties

        /// <summary>
        /// Text sandbox which is used to get or set the value from a dependency property.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }


        // Real implementation about TextProperty which  registers a dependency property with 
        // the specified property name, property type, owner type, and property metadata. 
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SearchableTextControl),
            new UIPropertyMetadata(string.Empty,
              UpdateControlCallBack));

        /// <summary>
        /// HighlightBackground sandbox which is used to get or set the value from a dependency property,
        /// if it gets a value,it should be forced to bind to a Brushes type.
        /// </summary>
        public Brush HighlightBackground
        {
            get { return (Brush)GetValue(HighlightBackgroundProperty); }
            set { SetValue(HighlightBackgroundProperty, value); }
        }


        // Real implementation about HighlightBackgroundProperty which registers a dependency property 
        // with the specified property name, property type, owner type, and property metadata. 
        public static readonly DependencyProperty HighlightBackgroundProperty =
            DependencyProperty.Register("HighlightBackground", typeof(Brush), typeof(SearchableTextControl),
            new UIPropertyMetadata(Brushes.Yellow, UpdateControlCallBack));

        /// <summary>
        /// HighlightForeground sandbox which is used to get or set the value from a dependency property,
        /// if it gets a value,it should be forced to bind to a Brushes type.
        /// </summary>
        public Brush HighlightForeground
        {
            get { return (Brush)GetValue(HighlightForegroundProperty); }
            set { SetValue(HighlightForegroundProperty, value); }
        }


        // Real implementation about HighlightForegroundProperty which registers a dependency property with
        // the specified property name, property type, owner type, and property metadata. 
        public static readonly DependencyProperty HighlightForegroundProperty =
            DependencyProperty.Register("HighlightForeground", typeof(Brush), typeof(SearchableTextControl),
            new UIPropertyMetadata(Brushes.Black, UpdateControlCallBack));

        /// <summary>
        /// IsMatchCase sandbox which is used to get or set the value from a dependency property,
        /// if it gets a value,it should be forced to bind to a bool type.
        /// </summary>
        public bool IsMatchCase
        {
            get { return (bool)GetValue(IsMatchCaseProperty); }
            set { SetValue(IsMatchCaseProperty, value); }
        }

        // Real implementation about IsMatchCaseProperty which  registers a dependency property with
        // the specified property name, property type, owner type, and property metadata. 
        public static readonly DependencyProperty IsMatchCaseProperty =
            DependencyProperty.Register("IsMatchCase", typeof(bool), typeof(SearchableTextControl),
            new UIPropertyMetadata(true, UpdateControlCallBack));

        /// <summary>
        /// IsHighlight sandbox which is used to get or set the value from a dependency property,
        /// if it gets a value,it should be forced to bind to a bool type.
        /// </summary>
        public bool IsHighlight
        {
            get { return (bool)GetValue(IsHighlightProperty); }
            set { SetValue(IsHighlightProperty, value); }
        }

        // Real implementation about IsHighlightProperty which  registers a dependency property with
        // the specified property name, property type, owner type, and property metadata. 
        public static readonly DependencyProperty IsHighlightProperty =
            DependencyProperty.Register("IsHighlight", typeof(bool), typeof(SearchableTextControl),
            new UIPropertyMetadata(false, UpdateControlCallBack));

        /// <summary>
        /// SearchText sandbox which is used to get or set the value from a dependency property,
        /// if it gets a value,it should be forced to bind to a string type.
        /// </summary>
        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        /// <summary>
        /// Real implementation about SearchTextProperty which registers a dependency property with
        /// the specified property name, property type, owner type, and property metadata. 
        /// </summary>
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(SearchableTextControl),
            new UIPropertyMetadata(string.Empty, UpdateControlCallBack));

        /// <summary>
        /// Create a call back function which is used to invalidate the rendering of the element, 
        /// and force a complete new layout pass.
        /// One such advanced scenario is if you are creating a PropertyChangedCallback for a 
        /// dependency property that is not  on a Freezable or FrameworkElement derived class that 
        /// still influences the layout when it changes.
        /// </summary>
        private static void UpdateControlCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SearchableTextControl obj = d as SearchableTextControl;
            obj.InvalidateVisual();
        }
        #endregion

        /// <summary>
        /// override the OnRender method which is used to search for the keyword and highlight
        /// it when the operation gets the result.
        /// </summary>
        protected override void OnRender(DrawingContext drawingContext)
        {

            // Define a TextBlock to hold the search result.
            TextBlock displayTextBlock = this.Template.FindName("PART_TEXT", this) as TextBlock;

            if (string.IsNullOrEmpty(this.Text))
            {
                base.OnRender(drawingContext);
      
                return;
            }
            if (!this.IsHighlight)
            {
                displayTextBlock.Text = this.Text;
                base.OnRender(drawingContext);
          
                return;
            }

            displayTextBlock.Inlines.Clear();
            string searchstring = this.IsMatchCase ? (string)this.SearchText : ((string)this.SearchText).ToUpper();

            string compareText = this.IsMatchCase ? this.Text : this.Text.ToUpper();
            string displayText = this.Text;

            Run run = null;
            while (!string.IsNullOrEmpty(searchstring) && compareText.IndexOf(searchstring) >= 0)
            {
                int position = compareText.IndexOf(searchstring);
                run = GenerateRun(displayText.Substring(0, position), false);

                if (run != null)
                {
                    displayTextBlock.Inlines.Add(run);
                }

                run = GenerateRun(displayText.Substring(position, searchstring.Length), true);

                if (run != null)
                {
                    displayTextBlock.Inlines.Add(run);
                }

                compareText = compareText.Substring(position + searchstring.Length);
                displayText = displayText.Substring(position + searchstring.Length);
            }

            run = GenerateRun(displayText, false);

            if (run != null)
            {
                displayTextBlock.Inlines.Add(run);
            }

         


            base.OnRender(drawingContext);
        }

        /// <summary>
        /// Set inline-level flow content element intended to contain a run of formatted or unformatted 
        /// text into your background and foreground setting.
        /// </summary>
        private Run GenerateRun(string searchedString, bool isHighlight)
        {
            if (!string.IsNullOrEmpty(searchedString))
            {
                Run run = new Run(searchedString)
                {
                    Background = isHighlight ? this.HighlightBackground : this.Background,
                    Foreground = isHighlight ? this.HighlightForeground : this.Foreground,

                    // Set the source text with the style which is Italic.
                 //   FontStyle = isHighlight ? FontStyles.Italic : FontStyles.Normal,

                    // Set the source text with the style which is Bold.
                    FontWeight = isHighlight ? FontWeights.Bold : FontWeights.Normal,
                };
                return run;
            }
            return null;
        }
    }

}
