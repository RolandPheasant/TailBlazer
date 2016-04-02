using System;
using System.Collections.Generic;
using System.Windows.Controls;
using DynamicData;
using TailBlazer.Views.Tail;

namespace TailBlazer.Infrastucture
{
    public interface IAttachedListBox
    {
        void Receive(ListBox selector);
    }


    public interface ISelectionMonitor: IDisposable
    {
        string GetSelectedText();

        IEnumerable<string> GetSelectedItems();

        IObservableList<LineProxy> Selected { get; }
    }
}