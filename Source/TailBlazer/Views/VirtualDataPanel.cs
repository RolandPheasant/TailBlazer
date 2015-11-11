using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DynamicData.Client.Infrastructure
{


    //public class ScrollViewer2: ScrollViewer
    //{
    //    override 
    //}

    public class BetterVirtualizingStackPanel : VirtualizingStackPanel  
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Windows.Controls.VirtualizingStackPanel"/> class.
        /// </summary>
        public BetterVirtualizingStackPanel()
        {
        //    base.ItemContainerGenerator.
        }

        #region Overrides of VirtualizingStackPanel

        /// <summary>
        /// Called when the size of the viewport changes.
        /// </summary>
        /// <param name="oldViewportSize">The old size of the viewport.</param><param name="newViewportSize">The new size of the viewport.</param>
        protected override void OnViewportSizeChanged(Size oldViewportSize, Size newViewportSize)
        {
            base.OnViewportSizeChanged(oldViewportSize, newViewportSize);
        }

       // #region Overrides of Panel

        /// <summary>
        /// Invoked when the <see cref="T:System.Windows.Media.VisualCollection"/> of a visual object is modified.
        /// </summary>
        /// <param name="visualAdded">The <see cref="T:System.Windows.Media.Visual"/> that was added to the collection.</param><param name="visualRemoved">The <see cref="T:System.Windows.Media.Visual"/> that was removed from the collection.</param>
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        #region Overrides of VirtualizingStackPanel

        /// <summary>
        /// Called when the <see cref="P:System.Windows.Controls.ItemsControl.Items"/> collection that is associated with the <see cref="T:System.Windows.Controls.ItemsControl"/> for this <see cref="T:System.Windows.Controls.Panel"/> changes.
        /// </summary>
        /// <param name="sender">The <see cref="T:System.Object"/> that raised the event.</param><param name="args">Provides data for the <see cref="E:System.Windows.Controls.ItemContainerGenerator.ItemsChanged"/> event.</param>
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            base.OnItemsChanged(sender, args);
        }

        #region Overrides of UIElement

        /// <summary>
        /// Provides class handling for when an access key that is meaningful for this element is invoked. 
        /// </summary>
        /// <param name="e">The event data to the access key event. The event data reports which key was invoked, and indicate whether the <see cref="T:System.Windows.Input.AccessKeyManager"/> object that controls the sending of these events also sent this access key invocation to other elements.</param>
     //   override on
        #endregion

        #endregion

        //  override 


        #endregion
    }


    public class VirtualDataPanel : VirtualizingPanel, IScrollInfo
    {
        private const double ScrollLineAmount = 16.0;
        private Size _extentSize;
        private ExtentInfo _extentInfo = new ExtentInfo();
        private Size _viewportSize;
        private Point _offset;
        private ItemsControl _itemsControl;
        private readonly Dictionary<UIElement, Rect> _childLayouts = new Dictionary<UIElement, Rect>();


        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register("ItemHeight", typeof(double), typeof(VirtualDataPanel), new PropertyMetadata(1.0, HandleItemDimensionChanged));

        
        private static readonly DependencyProperty VirtualItemIndexProperty =
            DependencyProperty.RegisterAttached("VirtualItemIndex", typeof(int), typeof(VirtualDataPanel), new PropertyMetadata(-1));

        public static readonly DependencyProperty TotalItemsProperty =
            DependencyProperty.Register("TotalItems", typeof (int), typeof (VirtualDataPanel), new PropertyMetadata(default(int),HandleItemDimensionChanged));
        
        public static readonly DependencyProperty StartIndexProperty =
            DependencyProperty.Register("StartIndex", typeof(int), typeof(VirtualDataPanel), new PropertyMetadata(default(int), HandleItemDimensionChanged));


        public static readonly DependencyProperty ChangeStartIndexCommandProperty =
            DependencyProperty.Register("ChangeStartIndexCommand", typeof(ICommand), typeof(VirtualDataPanel), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty ChangeSizeCommandProperty =
            DependencyProperty.Register("ChangeSizeCommand", typeof (ICommand), typeof (VirtualDataPanel), new PropertyMetadata(default(ICommand)));




        public ICommand ChangeSizeCommand
        {
            get { return (ICommand) GetValue(ChangeSizeCommandProperty); }
            set { SetValue(ChangeSizeCommandProperty, value); }
        }


        public ICommand ChangeStartIndexCommand
        {
            get { return (ICommand)GetValue(ChangeStartIndexCommandProperty); }
            set { SetValue(ChangeStartIndexCommandProperty, value); }
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
        
        
        private IRecyclingItemContainerGenerator _itemsGenerator;

        private bool _isInMeasure;

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

        public double ItemWidth
        {
            get
            {
                return _extentSize.Width;
            }
        }

        public VirtualDataPanel()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Dispatcher.BeginInvoke(new Action(Initialize));
            }
        }

        private void Initialize()
        {
            _itemsControl = ItemsControl.GetItemsOwner(this);
            _itemsGenerator = (IRecyclingItemContainerGenerator)ItemContainerGenerator;

            InvalidateMeasure();
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            base.OnItemsChanged(sender, args);

            InvalidateMeasure();
        }


        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.HeightChanged)
            {
                var items = (int)(sizeInfo.NewSize.Height / ItemHeight)+4;
                InvokeSizeCommand(items);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {

            if (_itemsControl == null)
            {
                return availableSize;
            }
            
            _isInMeasure = true;
            _childLayouts.Clear();

            var extentInfo = GetVerticalExtentInfo(availableSize);
            _extentInfo = extentInfo;

            EnsureScrollOffsetIsWithinConstrains(extentInfo);

          //  SetVerticalOffset(extentInfo.VerticalOffset);
            var layoutInfo = GetLayoutInfo(availableSize, ItemHeight, extentInfo);

            RecycleItems(layoutInfo);

            // Determine where the first item is in relation to previously realized items
            var generatorStartPosition = _itemsGenerator.GeneratorPositionFromIndex(layoutInfo.FirstRealizedItemIndex);

            var visualIndex = 0;
            double actualWidth = 0;
            var currentX = layoutInfo.FirstRealizedItemLeft;
            var currentY = layoutInfo.FirstRealizedLineTop;

            using (_itemsGenerator.StartAt(generatorStartPosition, GeneratorDirection.Forward, true))
            {
                for (var itemIndex = layoutInfo.FirstRealizedItemIndex; itemIndex <= layoutInfo.LastRealizedItemIndex; itemIndex++, visualIndex++)
                {
                    bool newlyRealized;

                    var child = (UIElement)_itemsGenerator.GenerateNext(out newlyRealized);
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
                            if (!Equals(Children[visualIndex], child))
                            {
                                var childCurrentIndex = Children.IndexOf(child);
                                if (childCurrentIndex >= 0)
                                {
                                    RemoveInternalChildRange(childCurrentIndex, 1);
                                }

                                InsertInternalChild(visualIndex, child);
                            }
                        }
                        else
                        {
                            // we know that the child can't already be in the children collection
                            // because we've been inserting children in correct visualIndex order,
                            // and this child has a visualIndex greater than the Children.Count
                            AddInternalChild(child);
                        }
                    }

                    // only prepare the item once it has been added to the visual tree
                    _itemsGenerator.PrepareItemContainer(child);
                    child.Measure(new Size(double.PositiveInfinity, ItemHeight));
                    actualWidth = Math.Max(actualWidth, child.DesiredSize.Width);

                    _childLayouts.Add(child, new Rect(currentX, currentY, actualWidth, ItemHeight));
                    currentY += ItemHeight;
                }
            }


           // Console.WriteLine(actualWidth);
            RemoveRedundantChildren();
            UpdateScrollInfo(availableSize, extentInfo,actualWidth);

            var desiredSize = new Size(double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
                                       double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);

            _isInMeasure = false;
            
            //if (_size!=desiredSize)
            //{
            //    _size = desiredSize;

            //    var xxx = (int)(desiredSize.Height/ItemHeight);
            //   InvokeSizeCommand(xxx + 5);

            //}
           


            return desiredSize;
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
                        _itemsGenerator.Recycle(generatorPosition, 1);
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
                LastRealizedItemIndex = lastRealizedIndex,
            };
        }

        private ExtentInfo GetVerticalExtentInfo(Size viewPortSize)
        {
            if (_itemsControl == null)
            {
                return new ExtentInfo();
            }

            var extentHeight = Math.Max(TotalItems * ItemHeight, viewPortSize.Height);

            var info = new ExtentInfo()
            {
                VirtualCount = _itemsControl.Items.Count,
                VerticalOffset = StartIndex * ItemHeight,
                TotalCount = TotalItems,
                Height = extentHeight,
                MaxVerticalOffset = extentHeight - viewPortSize.Height,
            };
            return info;
        }


        public void LineUp()
        {
            InvokeStartIndexCommand(-1);
        }

        public void LineDown()
        {
            InvokeStartIndexCommand(1);
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

           
            var diff = (int)((offset - _extentInfo.VerticalOffset) / ItemHeight);
            InvokeStartIndexCommand(diff);
        }



        private double Clamp(double value, double min, double max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        private void InvokeStartIndexCommand(int lines)
        {
            if (_isInMeasure) return;

            var firstIndex = StartIndex + lines;
            if (firstIndex<0)
            {
                firstIndex = 0;
            }
            else if (firstIndex + _extentInfo.VirtualCount >_extentInfo.TotalCount)
            {
                firstIndex = _extentInfo.TotalCount - _extentInfo.VirtualCount;
            }

            Dispatcher.BeginInvoke(new Action(() => ChangeStartIndexCommand.Execute(firstIndex)));

          //  InvalidateScrollInfo();
          //  InvalidateMeasure();
        }

        private void InvokeSizeCommand(int size)
        {
            if (ChangeSizeCommand == null) 
                return;

            //if (_isInMeasure)
            //{
            //    return;
            //}

            Dispatcher.BeginInvoke(new Action(() => ChangeSizeCommand.Execute(size)))
            ;
            //InvalidateScrollInfo();
           // InvalidateMeasure();
        }
        
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return new Rect();
        }


        public bool CanVerticallyScroll
        {
            get;
            set;
        }

        public bool CanHorizontallyScroll
        {
            get;
            set;
        }

        public double ExtentWidth
        {
            get
            {
                return _extentSize.Width;
            }
        }

        public double ExtentHeight
        {
            get { return _extentSize.Height; }
        }

        public double ViewportWidth
        {
            get { return _viewportSize.Width; }
        }

        public double ViewportHeight
        {
            get { return _viewportSize.Height; }
        }

        public double HorizontalOffset
        {
            get { return _offset.X; }
        }

        public double VerticalOffset
        {
            get { return _offset.Y + _extentInfo.VerticalOffset; }
        }

        public ScrollViewer ScrollOwner
        {
            get;
            set;
        }

        private void InvalidateScrollInfo()
        {
            if (ScrollOwner != null)
            {
                ScrollOwner.InvalidateScrollInfo();
            }
        }

        private static void HandleItemDimensionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wrapPanel = (d as VirtualDataPanel);

            wrapPanel.InvalidateMeasure();
        }




        internal class ExtentInfo
        {
            public int TotalCount;
            public int VirtualCount;
            public double VerticalOffset;
            public double MaxVerticalOffset;
            public double Height;

        }
    }


    public class ItemLayoutInfo
    {
        public int FirstRealizedItemIndex;
        public double FirstRealizedLineTop;
        public double FirstRealizedItemLeft;
        public int LastRealizedItemIndex;
    }
}
