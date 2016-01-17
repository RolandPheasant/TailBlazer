using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using TailBlazer.Infrastucture;

namespace TailBlazer.Controls
{
    public static class MeasureEx
    {
        public static Size MeasureString(this Control source,  string candidate)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(source.FontFamily, source.FontStyle, source.FontWeight, source.FontStretch),
                source.FontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }

    /// <summary>
    /// This is adapted (butchered!) from VirtualWrapPanel in https://github.com/samueldjack/VirtualCollection
    /// 
    /// See http://blog.hibernatingrhinos.com/12515/implementing-a-virtualizingwrappanel
    /// </summary>
    public class VirtualScrollPanel : VirtualizingPanel, IScrollInfo
    {
        private const double ScrollLineAmount = 16.0;
        private Size _extentSize;
        private ExtentInfo _extentInfo = new ExtentInfo();
        private Size _viewportSize;
        private Point _offset;
        private ItemsControl _itemsControl;
        private readonly Dictionary<UIElement, Rect> _childLayouts = new Dictionary<UIElement, Rect>();
        private IRecyclingItemContainerGenerator _itemsGenerator;
        private bool _isInMeasure;

        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register("ItemHeight", typeof(double), typeof(VirtualScrollPanel), new PropertyMetadata(1.0, OnRequireMeasure));

        
        private static readonly DependencyProperty VirtualItemIndexProperty =
            DependencyProperty.RegisterAttached("VirtualItemIndex", typeof(int), typeof(VirtualScrollPanel), new PropertyMetadata(-1));

        public static readonly DependencyProperty TotalItemsProperty =
            DependencyProperty.Register("TotalItems", typeof (int), typeof (VirtualScrollPanel), new PropertyMetadata(default(int),OnRequireMeasure));
        
        public static readonly DependencyProperty StartIndexProperty =
            DependencyProperty.Register("StartIndex", typeof(int), typeof(VirtualScrollPanel), new PropertyMetadata(default(int), OnStartIndexChanged));

        public static readonly DependencyProperty ScrollReceiverProperty = DependencyProperty.Register(
            "ScrollReceiver", typeof (IScrollReceiver), typeof (VirtualScrollPanel), new PropertyMetadata(default(IScrollReceiver)));

        public IScrollReceiver ScrollReceiver
        {
            get { return (IScrollReceiver) GetValue(ScrollReceiverProperty); }
            set { SetValue(ScrollReceiverProperty, value); }
        }

        public int StartIndex
        {
            get { return (int) GetValue(StartIndexProperty); }
            set { SetValue(StartIndexProperty, value); }
        }

        public int TotalItems
        {
            get { return (int) GetValue(TotalItemsProperty); }
            set { SetValue(TotalItemsProperty, value); }
        }
        
        private static int GetVirtualItemIndex(DependencyObject obj)
        {
            return (int)obj.GetValue(VirtualItemIndexProperty);
        }

        private static void SetVirtualItemIndex(DependencyObject obj, int value)
        {
            obj.SetValue(VirtualItemIndexProperty, value);
        }

        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }

        public double ItemWidth => _extentSize.Width;

        public VirtualScrollPanel()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                Dispatcher.BeginInvoke(new Action(Initialize));
        }

        private void Initialize()
        {
            _itemsControl = ItemsControl.GetItemsOwner(this);
            _itemsGenerator = (IRecyclingItemContainerGenerator)ItemContainerGenerator;


            _itemsControl.LostFocus += _itemsControl_LostFocus;
            _itemsControl.LostKeyboardFocus += _itemsControl_LostKeyboardFocus;
            InvalidateMeasure();
        }

        private void _itemsControl_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
         //   _itemsControl.Focus();
        }

        private void _itemsControl_LostFocus(object sender, RoutedEventArgs e)
        {
            //_itemsControl.Focus();
        }

        private static void OnStartIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (VirtualScrollPanel)d;
            panel. CallbackStartIndexChanged(Convert.ToInt32(e.NewValue));
            panel.InvalidateMeasure();
        }

        private static void OnRequireMeasure(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (VirtualScrollPanel)d;
            panel.InvalidateMeasure();
            panel.InvalidateScrollInfo();

        }
        
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            base.OnItemsChanged(sender, args);
            InvalidateMeasure();
        }
        
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (!sizeInfo.HeightChanged) return;

            var items = (int)(sizeInfo.NewSize.Height / ItemHeight) ;
            InvokeSizeCommand(items);

        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_itemsControl == null)
            {

                return new Size(double.IsInfinity(availableSize.Width) ? 0: availableSize.Width,
                    double.IsInfinity(availableSize.Height)  ? 0 : availableSize.Height);
            }

            
            _isInMeasure = true;
            _childLayouts.Clear();
            _extentInfo = GetVerticalExtentInfo(availableSize);

            EnsureScrollOffsetIsWithinConstrains(_extentInfo);
            var layoutInfo = GetLayoutInfo(availableSize, ItemHeight, _extentInfo);

            RecycleItems(layoutInfo);

            // Determine where the first item is in relation to previously realized items
            var generatorStartPosition = _itemsGenerator.GeneratorPositionFromIndex(layoutInfo.FirstRealizedItemIndex);

            var visualIndex = 0;
            double widestWidth = 0;
            var currentX = layoutInfo.FirstRealizedItemLeft;
            var currentY = layoutInfo.FirstRealizedLineTop;

            using (_itemsGenerator.StartAt(generatorStartPosition, GeneratorDirection.Forward, true))
            {
                var children = new List<UIElement>();

                for (var itemIndex = layoutInfo.FirstRealizedItemIndex; itemIndex <= layoutInfo.LastRealizedItemIndex; itemIndex++, visualIndex++)
                {
                    bool newlyRealized;

                    var child = (UIElement)_itemsGenerator.GenerateNext(out newlyRealized);

                    if (child==null) continue;


                    children.Add(child);

                    SetVirtualItemIndex(child, itemIndex);

                    if (newlyRealized)
                    {
                        InsertInternalChild(visualIndex, child);
                    }
                    else
                    {
                        // check if item needs to be moved into a new position in the Children collection
                        if (visualIndex < Children.Count)
                        {
                            if (Equals(Children[visualIndex], child)) continue;
                            var childCurrentIndex = Children.IndexOf(child);
                            if (childCurrentIndex >= 0)
                            {
                                RemoveInternalChildRange(childCurrentIndex, 1);
                            }

                            InsertInternalChild(visualIndex, child);
                        }
                        else
                        {
                            // we know that the child can't already be in the children collection
                            // because we've been inserting children in correct visualIndex order,
                            // and this child has a visualIndex greater than the Children.Count
                            AddInternalChild(child);
                        }
                    }
                }

                //part 2: do the measure
                foreach (var child in children)
                {


                    _itemsGenerator.PrepareItemContainer(child);
                    child.Measure(new Size(double.PositiveInfinity, ItemHeight));
                    widestWidth= Math.Max(widestWidth, child.DesiredSize.Width);
                }

                //part 3: Create the elements
                foreach (var child in children)
                {
                    _childLayouts.Add(child, new Rect(currentX, currentY, Math.Max(widestWidth,_viewportSize.Width), ItemHeight));
                    currentY += ItemHeight;
                }
            }
            RemoveRedundantChildren();
            UpdateScrollInfo(availableSize, _extentInfo, widestWidth);

            _isInMeasure = false;

            return new Size(double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
    double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
            //return availableSize;
        }

        private void EnsureScrollOffsetIsWithinConstrains(ExtentInfo extentInfo)
        {
            _offset.Y = Clamp(_offset.Y, 0, extentInfo.MaxVerticalOffset);
        }

        private void RecycleItems(ItemLayoutInfo layoutInfo)
        {
            foreach (UIElement child in Children)
            {
                var virtualItemIndex = GetVirtualItemIndex(child);

                if (virtualItemIndex < layoutInfo.FirstRealizedItemIndex || virtualItemIndex > layoutInfo.LastRealizedItemIndex)
                {
                    var generatorPosition = _itemsGenerator.GeneratorPositionFromIndex(virtualItemIndex);
                    if (generatorPosition.Index >= 0)
                    {
                        try
                        {
                            _itemsGenerator.Recycle(generatorPosition, 1);
                        }
                        catch (ArgumentException)
                        {
                            //I have seen the following exception which appears to be a non-issue
                            // GeneratorPosition '0,10' passed to Remove does not have Offset equal to 0.
                        }

                    }
                }
                SetVirtualItemIndex(child, -1);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in Children)
            {
                child.Arrange(_childLayouts[child]);
            }
            return finalSize;
        }

        private void UpdateScrollInfo(Size availableSize, ExtentInfo extentInfo, double actualWidth)
        {
            _viewportSize = availableSize;
            _extentSize = new Size(actualWidth, extentInfo.Height);

            InvalidateScrollInfo();
        }

        private void InvalidateScrollInfo()
        {
            ScrollOwner?.InvalidateScrollInfo();
        }

        private void RemoveRedundantChildren()
        {
            // iterate backwards through the child collection because we're going to be
            // removing items from it
            for (var i = Children.Count - 1; i >= 0; i--)
            {
                var child = Children[i];

                // if the virtual item index is -1, this indicates
                // it is a recycled item that hasn't been reused this time round
                if (GetVirtualItemIndex(child) == -1)
                {
                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        private ItemLayoutInfo GetLayoutInfo(Size availableSize, double itemHeight, ExtentInfo extentInfo)
        {
            if (_itemsControl == null)
            {
                return new ItemLayoutInfo();
            }

            // we need to ensure that there is one realized item prior to the first visible item, and one after the last visible item,
            // so that keyboard navigation works properly. For example, when focus is on the first visible item, and the user
            // navigates up, the ListBox selects the previous item, and the scrolls that into view - and this triggers the loading of the rest of the items 
            // in that row
            var firstVisibleLine = (int)Math.Floor(_offset.Y / itemHeight);
            var firstRealizedIndex = Math.Max(firstVisibleLine - 1, 0);
            var firstRealizedItemLeft = firstRealizedIndex  * ItemWidth - HorizontalOffset;
            var firstRealizedItemTop = (firstRealizedIndex ) * itemHeight - _offset.Y;
            var firstCompleteLineTop = (firstVisibleLine == 0 ? firstRealizedItemTop : firstRealizedItemTop + ItemHeight);
            var completeRealizedLines = (int)Math.Ceiling((availableSize.Height - firstCompleteLineTop) / itemHeight);

            var lastRealizedIndex = Math.Min(firstRealizedIndex + completeRealizedLines  + 2, _itemsControl.Items.Count - 1);

            return new ItemLayoutInfo
            {
                FirstRealizedItemIndex = firstRealizedIndex,
                FirstRealizedItemLeft = firstRealizedItemLeft,
                FirstRealizedLineTop = firstRealizedItemTop,
                LastRealizedItemIndex = lastRealizedIndex
            };
        }
        
        private ExtentInfo GetVerticalExtentInfo(Size viewPortSize)
        {
            if (_itemsControl == null)
                return new ExtentInfo();
        
            var extentHeight = Math.Max(TotalItems * ItemHeight, viewPortSize.Height);
            var maxVerticalOffset = extentHeight;// extentHeight - viewPortSize.Height;
            var verticalOffset = (StartIndex /(double) TotalItems)*maxVerticalOffset;

            var info = new ExtentInfo
            {
                VirtualCount = _itemsControl.Items.Count,
                TotalCount = TotalItems,
                Height = extentHeight,
                VerticalOffset = verticalOffset,
                MaxVerticalOffset = maxVerticalOffset
            };
            return info;
        }



        public void SetHorizontalOffset(double offset)
        {
            offset = Clamp(offset, 0, ExtentWidth - ViewportWidth);

            if (offset<0)
            {
                _offset.X = 0;
            }
            else
            {
                _offset = new Point(offset, _offset.Y);

            }

            InvalidateScrollInfo();
            InvalidateMeasure();
        }

        public void SetVerticalOffset(double offset)
        {
            if (double.IsInfinity(offset)) return;
            var diff = (int)((offset - _extentInfo.VerticalOffset) / ItemHeight);

         //   Console.WriteLine($"Scrolling by {diff} lines");
            InvokeStartIndexCommand(diff);
        }

        
        private double Clamp(double value, double min, double max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        private int _firstIndex;
        private int _size;

        private void InvokeStartIndexCommand(int lines)
        {

            if (_isInMeasure) return;

            var firstIndex = StartIndex + lines;
            if (firstIndex<0)
            {
                firstIndex = 0;
            }
            else if (firstIndex + _extentInfo.VirtualCount >= _extentInfo.TotalCount)
            {
                firstIndex = _extentInfo.TotalCount - _extentInfo.VirtualCount;
            }

            if (firstIndex == _firstIndex) return;

            if (_firstIndex == firstIndex) return;
            _firstIndex = firstIndex;

            OnOffsetChanged(lines > 0 ? ScrollDirection.Down : ScrollDirection.Up, lines);
            ReportChanges();
        }

        private void ReportChanges()
        {
            ScrollReceiver?.ScrollBoundsChanged(new ScrollBoundsArgs(_size, _firstIndex));
        }

        private void OnOffsetChanged(ScrollDirection direction,int firstRow)
        {
            ScrollReceiver?.ScrollChanged(new ScrollChangedArgs(direction, firstRow));
        }

        private void CallbackStartIndexChanged(int index)
        {
            if (_firstIndex == index) return;
            _firstIndex = index;
           ReportChanges();
        }
 
        private void InvokeSizeCommand(int size)
        {
            if (_size == size) return;
            _size = size;
            ReportChanges();
        }

        public bool CanVerticallyScroll {get;set;}
        public bool CanHorizontallyScroll {get;set;}
        public double ExtentWidth => _extentSize.Width;
        public double ExtentHeight => _extentSize.Height;
        public double ViewportWidth => _viewportSize.Width;
        public double ViewportHeight => _viewportSize.Height;
        public double HorizontalOffset => _offset.X;
        public double VerticalOffset => _offset.Y + _extentInfo.VerticalOffset;
        public ScrollViewer ScrollOwner {get;set;}

        public void LineUp()
        {
           // InvokeStartIndexCommand(-1);
        }

        public void LineDown()
        {
          //  InvokeStartIndexCommand(1);
        }

        public void LineLeft()
        {
            SetHorizontalOffset(HorizontalOffset + ScrollLineAmount);
        }

        public void LineRight()
        {
            SetHorizontalOffset(HorizontalOffset - ScrollLineAmount);
        }

        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - ViewportHeight);
        }

        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + ViewportHeight);
        }

        public void PageLeft()
        {
            SetHorizontalOffset(HorizontalOffset + ItemWidth);
        }

        public void PageRight()
        {
            SetHorizontalOffset(HorizontalOffset - ItemWidth);
        }

        public void MouseWheelUp()
        {
            InvokeStartIndexCommand(-SystemParameters.WheelScrollLines);
        }

        public void MouseWheelDown()
        {
            InvokeStartIndexCommand(SystemParameters.WheelScrollLines);
        }

        public void MouseWheelLeft()
        {
            SetHorizontalOffset(HorizontalOffset - ScrollLineAmount * SystemParameters.WheelScrollLines);
        }

        public void MouseWheelRight()
        {
            SetHorizontalOffset(HorizontalOffset + ScrollLineAmount * SystemParameters.WheelScrollLines);
        }
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return new Rect();
        }

        private class ItemLayoutInfo
        {
            public int FirstRealizedItemIndex;
            public double FirstRealizedLineTop;
            public double FirstRealizedItemLeft;
            public int LastRealizedItemIndex;
        }


        private class ExtentInfo
        {
            public int TotalCount;
            public int VirtualCount;
            public double VerticalOffset;
            public double MaxVerticalOffset;
            public double Height;

        }
    }

}
