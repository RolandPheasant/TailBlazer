using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dragablz;
using Dragablz.Dockablz;
using TailBlazer.Domain.Settings;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views.Layout
{

    public class StateNodeInterpreter
    {
       
    }

   

    public class BranchNode
    {
        public string Orientation { get; }
        public double Ratio { get; }

        public BranchNode(double ratio)
        {
            Ratio = ratio;
        }

        public BranchNode(string orientation)
        {
            Orientation = orientation;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Orientation))
                return $"Branch {Orientation}";

            return $"Proportion {Ratio}";
        }
    }


    
    public class LayoutAnalyser
    {
        private readonly StateNode _rootNode;

        public LayoutAnalyser()
        {;
            _rootNode = new StateNode("Application");
        }

        public IEnumerable<StateNode> QueryLayouts()
        {
            _rootNode.Children.Clear();

            foreach (var w in Application.Current.Windows.OfType<MainWindow>())
            {
                var bounds = w.RestoreBounds;
               
                var shellState = new ShellSettings(bounds.Top, 
                            bounds.Left, 
                            bounds.Right- bounds.Left,
                            bounds.Top - bounds.Bottom, 
                            w.WindowState);

                var layoutAccessor = w.Layout.Query();
                var layoutNode = new StateNode(shellState);
                _rootNode.Children.Add(layoutNode);

             //   FloatingItemsVisitor(layoutNode, layoutAccessor);
                layoutAccessor.Visit(layoutNode, BranchAccessorVisitor, TabablzControlVisitor);
            }

            return new[] {_rootNode};
        }


        private static void FloatingItemsVisitor(StateNode layoutNode, LayoutAccessor layoutAccessor)
        {
            var floatingItems = layoutAccessor.FloatingItems.ToList();
            var floatingItemsNode = new StateNode("Floating Items " + floatingItems.Count);
            foreach (var floatingItemNode in floatingItems.Select(floatingItem => new StateNode
            (
                $"Floating Item {floatingItem.X}, {floatingItem.Y} : {floatingItem.ActualWidth}, {floatingItem.ActualHeight}"
            )))
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

            var tabablzNode = new StateNode(tabStates);
            stateNode.Children.Add(tabablzNode);
        }

        private static void BranchAccessorVisitor(StateNode stateNode, BranchAccessor branchAccessor)
        {
            
            var branchNode = new StateNode( new BranchNode(branchAccessor.Branch.Orientation.ToString()) );
            stateNode.Children.Add(branchNode);

            var proportion = branchAccessor.Branch.GetFirstProportion();

            var firstBranchNode = new StateNode(new BranchNode(proportion));
            branchNode.Children.Add(firstBranchNode);
            var secondBranchNode = new StateNode(new BranchNode(1-proportion));
            branchNode.Children.Add(secondBranchNode);

            branchAccessor
                .Visit(firstBranchNode, BranchItem.First, BranchAccessorVisitor, TabablzControlVisitor)
                .Visit(secondBranchNode, BranchItem.Second, BranchAccessorVisitor, TabablzControlVisitor);
        }
    }
}
