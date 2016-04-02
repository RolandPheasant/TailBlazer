using System;
using TailBlazer.Domain.FileHandling;

namespace TailBlazer.Views.Tail
{
    public interface IInlineViewerFactory
    {
        InlineViewer Create(IObservable<ILineProvider> lineProvider,IObservable<LineProxy> selectedChanged, ILineProxyFactory lineProxyFactory);
    }
}