using System;
using System.Collections.Generic;
using System.Windows.Controls;
using DynamicData;
using TailBlazer.Views;

namespace TailBlazer.Infrastucture
{
    public interface IAttachedListBox
    {
        void Receive(ListBox selector);
    }


    public class LineViewer: ListBox 
    {
        
    }


    public interface ISelectionMonitor: IDisposable
    {
        string GetSelectedText();

        IEnumerable<string> GetSelectedItems();

        IObservableList<LineProxy> Selected { get; }
    }
}