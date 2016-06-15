using System;
using System.Collections.ObjectModel;
using Dragablz;

namespace TailBlazer.Views.Searching
{
    public interface ISearchProxyCollection: IDisposable
    {
        ReadOnlyObservableCollection<SearchOptionsProxy> Data { get; }
        VerticalPositionMonitor PositionMonitor { get; }
    }
}