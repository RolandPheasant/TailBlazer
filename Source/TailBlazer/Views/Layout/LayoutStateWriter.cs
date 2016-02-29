using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Dragablz;
using Dragablz.Dockablz;
using TailBlazer.Domain.Settings;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views.Layout
{
    //Store:

    //1. -Root = string only
    //2.  --Shell [size etc]
    //3.  --Branch [proportion within tab page]
    //4.  --View details [view state is passed ]

    public class LayoutConverter : ILayoutConverter
    {
        private static class XmlStructure
        {
            public const string Root = "LayoutRoot";

            public static class BranchNode
            {
                public const string Branches = "Branches";
                public const string Branch = "Branch";
                public const string Orientation = "Orientation";
                public const string Proportion = "Proportion";
            }

            public static class ViewNode
            {
                public const string Children = "Children";
                public const string ViewState = "ViewState";
                public const string Version = "Version";
                public const string Key = "Key";
            }
        }

        public XElement CaptureState()
        {
            var root = new XElement(XmlStructure.Root);
            
            foreach (var w in Application.Current.Windows.OfType<MainWindow>())
            {
                var bounds = w.RestoreBounds;

                var shellState = new ShellSettings(bounds.Top,
                            bounds.Left,
                            bounds.Right - bounds.Left,
                            bounds.Bottom - bounds.Top,
                            w.WindowState);

                var converter = new ShellStateConverter();
                var shellNode = converter.Convert(shellState);
                root.Add(shellNode);

                //add children to shell node
                var layoutAccessor = w.Layout.Query();
                layoutAccessor.Visit(shellNode, BranchAccessorVisitor, TabablzControlVisitor);
            }

            return root;
        }

        private static void TabablzControlVisitor(XElement stateNode, TabablzControl tabablzControl)
        {
            var tabStates = tabablzControl.Items.OfType<ViewContainer>()
                .Select(item => item.Content).OfType<IPersistentView>()
                .Select(provider => provider.CaptureState())
                .Select(state =>
                {
                    var viewState = new XElement(XmlStructure.ViewNode.ViewState,new XAttribute(XmlStructure.ViewNode.Key, state.Key));
                    viewState.SetAttributeValue(XmlStructure.ViewNode.Version, state.State.Version);
                    viewState.Add(state.State.Value);
                    return viewState;
                });
            
            var elements = new XElement(XmlStructure.ViewNode.Children, tabStates);
            stateNode.Add(elements);
        }

        private static void BranchAccessorVisitor(XElement stateNode, BranchAccessor branchAccessor)
        {
            var proportion = branchAccessor.Branch.GetFirstProportion();
            var firstBranch = new XElement(XmlStructure.BranchNode.Branch, new XAttribute(XmlStructure.BranchNode.Proportion, proportion));
            var secondBranch = new XElement(XmlStructure.BranchNode.Branch, new XAttribute(XmlStructure.BranchNode.Proportion, 1 - proportion));

            var branchNode = new XElement(XmlStructure.BranchNode.Branches, new XAttribute(XmlStructure.BranchNode.Orientation, branchAccessor.Branch.Orientation.ToString()));
            branchNode.Add(firstBranch);
            branchNode.Add(secondBranch);

            stateNode.Add(branchNode);

            branchAccessor
                .Visit(firstBranch, BranchItem.First, BranchAccessorVisitor, TabablzControlVisitor)
                .Visit(secondBranch, BranchItem.Second, BranchAccessorVisitor, TabablzControlVisitor);
        }
    }
}
