using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Dragablz;
using Dragablz.Dockablz;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Settings;
using TailBlazer.Infrastucture;
using TailBlazer.Views.WindowManagement;

namespace TailBlazer.Views.Layout
{
    //Store:

    //1. -Root = string only
    //2.  --Shell [size etc]
    //3.  --Branch [proportion within tab page]
    //4.  --View details [view state is passed ]
    public class LayoutConverter : ILayoutConverter
    {
        private readonly IWindowFactory _windowFactory;
        private readonly IViewFactoryProvider _viewFactoryProvider;

        private static class XmlStructure
        {
            public const string Root = "LayoutRoot";

            public static class ShellNode
            {
                public const string Shells = "Shells";
                public const string Shell = "Shell";
                public const string WindowsState = "WindowsState";
                public const string Top = "Top";
                public const string Left = "Left";
                public const string Width = "Width";
                public const string Height = "Height";
            }

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

        public LayoutConverter(IWindowFactory windowFactory, IViewFactoryProvider viewFactoryProvider)
        {
            _windowFactory = windowFactory;
            _viewFactoryProvider = viewFactoryProvider;
        }

        #region Capture state
        
        public XElement CaptureState()
        {
            var root = new XElement(XmlStructure.Root);
            var shells = new XElement(XmlStructure.ShellNode.Shells);
            
            foreach (var w in Application.Current.Windows.OfType<MainWindow>())
            {
                var bounds = w.RestoreBounds;
                var shellNode = new XElement(new XElement(XmlStructure.ShellNode.Shell));
                shellNode.Add(new XElement(XmlStructure.ShellNode.WindowsState, w.WindowState));
                shellNode.Add(new XElement(XmlStructure.ShellNode.Top, bounds.Top));
                shellNode.Add(new XElement(XmlStructure.ShellNode.Left, bounds.Left));
                shellNode.Add(new XElement(XmlStructure.ShellNode.Width, bounds.Right - bounds.Left));
                shellNode.Add(new XElement(XmlStructure.ShellNode.Height, bounds.Bottom - bounds.Top));

                shells.Add(shellNode);

                //add children to shell node
                var layoutAccessor = w.Layout.Query();
                layoutAccessor.Visit(shellNode, BranchAccessorVisitor, TabablzControlVisitor);
            }

            root.Add(shells);
            return root;
        }

        private static void TabablzControlVisitor(XElement stateNode, TabablzControl tabablzControl)
        {
            var tabStates = tabablzControl.Items.OfType<HeaderedView>()
                .Select(item => item.Content).OfType<IPersistentView>()
                .Select(provider => provider.CaptureState())
                .Select(state =>
                {
                    var viewState = new XElement(XmlStructure.ViewNode.ViewState,new XAttribute(XmlStructure.ViewNode.Key, state.Key));
                    viewState.SetAttributeValue(XmlStructure.ViewNode.Version, state.State.Version);
                    viewState.Add(state.State.Value);
                    return viewState;
                }).ToArray();
            
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

        #endregion

        #region Restore state

        public void Restore([NotNull] XElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            element.Elements(XmlStructure.ShellNode.Shells)
                .SelectMany(shells => shells.Elements(XmlStructure.ShellNode.Shell))
                .Select((shellState, index) =>
                {
                    var winState = shellState.ElementOrThrow(XmlStructure.ShellNode.WindowsState).ParseEnum<WindowState>().Value;
                    var top = shellState.ElementOrThrow(XmlStructure.ShellNode.Top).ParseDouble().Value;
                    var left = shellState.ElementOrThrow(XmlStructure.ShellNode.Left).ParseDouble().Value;
                    var width = shellState.ElementOrThrow(XmlStructure.ShellNode.Width).ParseDouble().Value;
                    var height = shellState.ElementOrThrow(XmlStructure.ShellNode.Height).ParseDouble().Value;
                    
                    var main = Application.Current.Windows.OfType<MainWindow>().First();
                    var window = index == 0 ? main : _windowFactory.Create();

                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                    window.WindowState = winState;
                    window.Left = left;
                    window.Top = top;
                    window.Width = width;
                    window.Height = height;

                    window.Show();
                    return new {window, shellState};
                })
                .ForEach(x =>
                {
                    RestoreBranches(x.window, x.shellState);
                    RestoreChildren(x.window, x.window.InitialTabablzControl, x.shellState);
                });
        }

        private void RestoreBranches(Window window, XElement element)
        {
            var branchNodes = element.Elements(XmlStructure.BranchNode.Branches)
                .Select(branch =>
                {
                    return branch;
                })
                .ToArray();
        }

        private void RestoreChildren(MainWindow window, XElement element)
        {
             element.Elements(XmlStructure.ViewNode.Children)
                .SelectMany(shells => shells.Elements(XmlStructure.ViewNode.ViewState))
                .Select(viewStateElement =>
                {
                    //1, Need factory for specific view
                    //2. Need factory to add view to specific tabablzControl
                    var key = viewStateElement.AttributeOrThrow(XmlStructure.ViewNode.Key);
                    var version = viewStateElement.AttributeOrThrow(XmlStructure.ViewNode.Version).ParseInt().Value;
                    var state = viewStateElement.Value;
                    var viewstate = new ViewState(key, new State(version, state));
                    return viewstate;
                })
                .ToArray()
                .ForEach(state =>
                {
                    var key = state.Key;
                    
                    var factory = _viewFactoryProvider.Lookup(key);

                    if (!factory.HasValue)
                        return;

                    //NEED TO GET A BETTER HANDLE ON WINDOWS CONTROLLER - Currently done via WindowsViewModel
                    var windowViewModel = (WindowViewModel)window.DataContext;
                    Task.Factory.StartNew(() =>
                    {
                            //this sucks because I am directly casting
                            var view = factory.Value.Create(state);
                            windowViewModel.OpenView(view);
                    });

                   


                });
        }

        #endregion



    }

    
}
