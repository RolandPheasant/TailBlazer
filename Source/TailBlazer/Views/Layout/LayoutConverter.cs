using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Dragablz;
using Dragablz.Dockablz;
using DynamicData.Kernel;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Infrastucture;
using TailBlazer.Views.WindowManagement;
using TailBlazer.Views.Options;

namespace TailBlazer.Views.Layout;
//Store:

//1. -Root = string only
//2.  --Shell [size etc]
//3.  --Branch [proportion within tab page]
//4.  --View details [view state is passed ]
public class LayoutConverter : ILayoutConverter
{
    private readonly IWindowFactory _windowFactory;
    private readonly IViewFactoryProvider _viewFactoryProvider;
    private readonly ISchedulerProvider _schedulerProvider;
    private readonly GeneralOptionsViewModel _generalOptionsViewModel;

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

    public LayoutConverter([NotNull] IWindowFactory windowFactory,
        [NotNull] IViewFactoryProvider viewFactoryProvider,
        [NotNull] ISchedulerProvider schedulerProvider,
        [NotNull] GeneralOptionsViewModel generalOptionsViewModel)
    {
        _windowFactory = windowFactory ?? throw new ArgumentNullException(nameof(windowFactory));
        _viewFactoryProvider = viewFactoryProvider ?? throw new ArgumentNullException(nameof(viewFactoryProvider));
        _schedulerProvider = schedulerProvider ?? throw new ArgumentNullException(nameof(schedulerProvider));
        _generalOptionsViewModel = generalOptionsViewModel ?? throw new ArgumentNullException(nameof(generalOptionsViewModel));
    }

    #region Capture state

    public XElement CaptureState()
    {
        var root = new XElement(XmlStructure.Root);
        var shells = new XElement(XmlStructure.ShellNode.Shells);

        foreach (var window in Application.Current.Windows.OfType<MainWindow>())
        {
            var bounds = window.RestoreBounds;
            var shellNode = new XElement(new XElement(XmlStructure.ShellNode.Shell));
            shellNode.Add(new XElement(XmlStructure.ShellNode.WindowsState, window.WindowState));
            shellNode.Add(new XElement(XmlStructure.ShellNode.Top, bounds.Top));
            shellNode.Add(new XElement(XmlStructure.ShellNode.Left, bounds.Left));
            shellNode.Add(new XElement(XmlStructure.ShellNode.Width, bounds.Right - bounds.Left));
            shellNode.Add(new XElement(XmlStructure.ShellNode.Height, bounds.Bottom - bounds.Top));

            shells.Add(shellNode);

            //add children to shell node
            var layoutAccessor = window.Layout.Query();
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
                var viewState = new XElement(XmlStructure.ViewNode.ViewState, new XAttribute(XmlStructure.ViewNode.Key, state.Key));
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
                return new { window, shellState };
            })
            .ForEach(x =>
            {
                RestoreBranches(x.window, x.shellState);
                    
                if (_generalOptionsViewModel.OpenRecentOnStartup)
                    RestoreChildren(x.window, x.shellState);
            });
    }

    private void RestoreBranches(MainWindow window, XElement element)
    {
        var tabControl = window.InitialTabablzControl;

        element.Elements(XmlStructure.BranchNode.Branches)
            .ForEach(branch =>
            {
                var orientaton = branch.AttributeOrThrow(XmlStructure.BranchNode.Orientation)
                    .ParseEnum<Orientation>()
                    .ValueOr(() => Orientation.Horizontal);
                var childBranches = branch.Elements(XmlStructure.BranchNode.Branch).ToArray();
                var firstBranch = childBranches.ElementAt(0);
                var secondBranch = childBranches.ElementAt(1);
                    
                var proportion = firstBranch.AttributeOrThrow(XmlStructure.BranchNode.Proportion)
                    .ParseDouble()
                    .ValueOr(() => 0.5);

                var firstChildList = GetViews(firstBranch);
                var secondChildList = GetViews(secondBranch);
                    
                var windowViewModel = (IViewOpener)window.DataContext;
                foreach (var headeredView in firstChildList.Union(secondChildList))
                {
                    windowViewModel.OpenView(headeredView);
                }

                //TODO: Sort this out. Throws an exception when I try to create a branch
                //var branchResult = Dragablz.Dockablz.Layout.Branch(tabControl, orientaton, false, proportion);
                    
                //Create branches + add children to branche
                //branchResult.TabablzControl.AddToSource(newItem);
                //branchResult.TabablzControl.SelectedItem = newItem;
            });
    }
        
    private void RestoreChildren(MainWindow window, XElement element)
    {
        //NEED TO GET A BETTER HANDLE ON WINDOWS CONTROLLER - Currently done via WindowsViewModel
        var windowViewModel = (IViewOpener)window.DataContext;

        GetViews(element)
            .ForEach(view =>
            {
                windowViewModel.OpenView(view);
            });
    }

    private IEnumerable<HeaderedView> GetViews(XElement element)
    {
        return GetChildrenState(element)
            .AsParallel()
            .AsOrdered()
            .Select(state =>
            {
                var key = state.Key;
                var factory = _viewFactoryProvider.Lookup(key);
                return !factory.HasValue ? null : factory.Value.Create(state);

            }).Where(view => view != null)
            .ToArray();
    }

    private IEnumerable<ViewState> GetChildrenState(XElement element)
    {
        return element.Elements(XmlStructure.ViewNode.Children)
            .SelectMany(shells => shells.Elements(XmlStructure.ViewNode.ViewState))
            .Select(viewStateElement =>
            {
                var key = viewStateElement.AttributeOrThrow(XmlStructure.ViewNode.Key);
                var version = viewStateElement.AttributeOrThrow(XmlStructure.ViewNode.Version).ParseInt().Value;
                var state = viewStateElement.Value;
                var viewstate = new ViewState(key, new State(version, state));
                return viewstate;
            });
    }

    #endregion
}