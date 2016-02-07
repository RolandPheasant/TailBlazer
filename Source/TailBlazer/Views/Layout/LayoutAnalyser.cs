using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dragablz;
using Dragablz.Dockablz;
using TailBlazer.Domain.Settings;
using TailBlazer.Infrastucture;


namespace TailBlazer.Views.Layout
{
    public class LayoutAnalyser
    {
        private readonly StateNode _rootNode;

        public LayoutAnalyser()
        {
            _rootNode = new StateNode
            {
                Content = "Application"
            };
        }

        public IEnumerable<StateNode> QueryLayouts()
        {
            _rootNode.Children.Clear();

            foreach (var w in Application.Current.Windows.OfType<MainWindow>())
            {
                var bounds = w.RestoreBounds;
               
                var shellState = new ShellState(bounds.Top, 
                            bounds.Left, 
                            bounds.Right- bounds.Left,
                            bounds.Top - bounds.Bottom, 
                            w.WindowState);

                var layoutAccessor = w.Layout.Query();
                var layoutNode = new StateNode
                {
                    Content = shellState
                };
                _rootNode.Children.Add(layoutNode);

                FloatingItemsVisitor(layoutNode, layoutAccessor);
                layoutAccessor.Visit(layoutNode, BranchAccessorVisitor, TabablzControlVisitor);
            }

            return new[] {_rootNode};
        }


        private static void FloatingItemsVisitor(StateNode layoutNode, LayoutAccessor layoutAccessor)
        {
            var floatingItems = layoutAccessor.FloatingItems.ToList();
            var floatingItemsNode = new StateNode {Content = "Floating Items " + floatingItems.Count};
            foreach (var floatingItemNode in floatingItems.Select(floatingItem => new StateNode
            {
                Content = $"Floating Item {floatingItem.X}, {floatingItem.Y} : {floatingItem.ActualWidth}, {floatingItem.ActualHeight}"
            }))

            {
                floatingItemsNode.Children.Add(floatingItemNode);
            }

            if (floatingItemsNode.Children.Count != 0)
                layoutNode.Children.Add(floatingItemsNode);
        }

        private static void TabablzControlVisitor(StateNode stateNode, TabablzControl tabablzControl)
        {
            var tabStates = tabablzControl.Items.OfType<ViewContainer>()
                .Select(item => item.Content).OfType<IPersistentStateProvider>()
                .Select(provider => provider.CaptureState())
                .ToList();

            Console.WriteLine(tabStates.Count);

            var tabablzNode = new StateNode
            {
                Content = tabStates
            };
            stateNode.Children.Add(tabablzNode);
        }

        private static void BranchAccessorVisitor(StateNode stateNode, BranchAccessor branchAccessor)
        {
            var branchNode = new StateNode {Content = "Branch " + branchAccessor.Branch.Orientation};
            stateNode.Children.Add(branchNode);

            var firstBranchNode = new StateNode {Content = "Branch Item 1. Ratio=" + branchAccessor.Branch.GetFirstProportion()};
            branchNode.Children.Add(firstBranchNode);
            var secondBranchNode = new StateNode {Content = "Branch Item 2. Ratio=" + (1 - branchAccessor.Branch.GetFirstProportion())};
            branchNode.Children.Add(secondBranchNode);

            branchAccessor.Visit(firstBranchNode, BranchItem.First, BranchAccessorVisitor, TabablzControlVisitor).Visit(secondBranchNode, BranchItem.Second, BranchAccessorVisitor, TabablzControlVisitor);
        }
    }
}
