using System;
using System.Windows;

using System.Xml.Linq;
using DynamicData.Kernel;
using TailBlazer.Domain.Settings;
using TailBlazer.Views.Options;

namespace TailBlazer.Views.Layout
{
    public class ShellStateConverter
    {
        private static class Structure
        {
            public const string Root = "Shell";
            public const string WindowsState = "WindowsState";
            public const string Top = "Top";
            public const string Left = "Left";
            public const string Width = "Width";
            public const string Height = "Height";
        }

        public ShellSettings Convert(XElement state)
        {
            var doc = XDocument.Parse(state.Value);
            var root = doc.ElementOrThrow(Structure.Root);
            var winState = root.ElementOrThrow(Structure.WindowsState).ParseEnum<WindowState>().Value;
            var top = root.ElementOrThrow(Structure.Top).ParseDouble().Value;
            var left = root.ElementOrThrow(Structure.Left).ParseDouble().Value;
            var width = root.ElementOrThrow(Structure.Width).ParseDouble().Value;
            var height = root.ElementOrThrow(Structure.Height).ParseDouble().Value;

            return new ShellSettings(top,left,width,height, winState);

        }

        public XElement Convert(ShellSettings state)
        {
            var root = new XElement(new XElement(Structure.Root));
            root.Add(new XElement(Structure.WindowsState, state.State));
            root.Add(new XElement(Structure.Top, state.Top));
            root.Add(new XElement(Structure.Left, state.Left));
            root.Add(new XElement(Structure.Width, state.Width));
            root.Add(new XElement(Structure.Height, state.Height));
            return root;
        }

    }
}
