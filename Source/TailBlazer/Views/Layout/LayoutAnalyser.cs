using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dragablz;
using Dragablz.Dockablz;



namespace TailBlazer.Views.Layout
{
    public class TabablzControlProxy : INotifyPropertyChanged
    {
        private readonly TabablzControl _tabablzControl;
        private readonly ICommand _splitHorizontallyCommand;
        private readonly ICommand _splitVerticallyCommand;
        private double _splitRatio;

        public TabablzControlProxy(TabablzControl tabablzControl)
        {
            _tabablzControl = tabablzControl;

            //_splitHorizontallyCommand = new AnotherCommandImplementation(_ => Branch(Orientation.Horizontal));
            //_splitVerticallyCommand = new AnotherCommandImplementation(_ => Branch(Orientation.Vertical));
            SplitRatio = 5;
        }

        public ICommand SplitHorizontallyCommand
        {
            get { return _splitHorizontallyCommand; }
        }

        public ICommand SplitVerticallyCommand
        {
            get { return _splitVerticallyCommand; }
        }

        public double SplitRatio
        {
            get { return _splitRatio; }
            set
            {
                _splitRatio = value;
                OnPropertyChanged("SplitRatio");
            }
        }

        //private void Branch(Orientation orientation)
        //{
        //    var branchResult = Layout.Branch(_tabablzControl, orientation, false, SplitRatio / 10);

        //    var newItem = new HeaderedItemViewModel
        //    {
        //        Header = "Code-Wise",
        //        Content = "This item was added in via code, using Layout.Branch, and TabablzControl.AddToSource"
        //    };

        //    branchResult.TabablzControl.AddToSource(newItem);
        //    branchResult.TabablzControl.SelectedItem = newItem;
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class LayoutAnalyser
    {
        private readonly TreeNode _rootNode;

        public LayoutAnalyser()
        {
            _rootNode = new TreeNode
            {
                Content = "Application"
            };
        }

        public IEnumerable<TreeNode> QueryLayouts()
        {
            _rootNode.Children.Clear();

            foreach (var layout in Application.Current.Windows.OfType<MainWindow>().Select(w => w.Layout))
            {
                var layoutAccessor = layout.Query();
                var layoutNode = new TreeNode
                {
                    Content = "Layout"
                };
                _rootNode.Children.Add(layoutNode);

                FloatingItemsVisitor(layoutNode, layoutAccessor);
                layoutAccessor.Visit(layoutNode, BranchAccessorVisitor, TabablzControlVisitor);
            }

            return new[] { _rootNode };
        }

        private static void FloatingItemsVisitor(TreeNode layoutNode, LayoutAccessor layoutAccessor)
        {
            var floatingItems = layoutAccessor.FloatingItems.ToList();
            var floatingItemsNode = new TreeNode { Content = "Floating Items " + floatingItems.Count };
            foreach (var floatingItemNode in floatingItems.Select(floatingItem => new TreeNode
            {
                Content = $"Floating Item {floatingItem.X}, {floatingItem.Y} : {floatingItem.ActualWidth}, {floatingItem.ActualHeight}"
            }))

            {
                    floatingItemsNode.Children.Add(floatingItemNode);
            }

            if (floatingItemsNode.Children.Count!=0)
                layoutNode.Children.Add(floatingItemsNode);
        }

        private static void TabablzControlVisitor(TreeNode treeNode, TabablzControl tabablzControl)
        {
            treeNode.Children.Add(new TreeNode { Content = new TabablzControlProxy(tabablzControl) });
        }

        private static void BranchAccessorVisitor(TreeNode treeNode, BranchAccessor branchAccessor)
        {
            var branchNode = new TreeNode { Content = "Branch " + branchAccessor.Branch.Orientation };
            treeNode.Children.Add(branchNode);

            var firstBranchNode = new TreeNode { Content = "Branch Item 1. Ratio=" + branchAccessor.Branch.GetFirstProportion() };
            branchNode.Children.Add(firstBranchNode);
            var secondBranchNode = new TreeNode { Content = "Branch Item 2. Ratio=" + (1 - branchAccessor.Branch.GetFirstProportion()) };
            branchNode.Children.Add(secondBranchNode);

            branchAccessor
                .Visit(firstBranchNode, BranchItem.First, BranchAccessorVisitor, TabablzControlVisitor)
                .Visit(secondBranchNode, BranchItem.Second, BranchAccessorVisitor, TabablzControlVisitor);
        }
    }
}
