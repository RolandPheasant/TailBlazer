using System;
using TailBlazer.Domain.FileHandling;

namespace TailBlazer.Views
{
    public interface IInlineViewerFactory
    {
        InlineViewer Create(IObservable<ILineProvider> lineProvider,IObservable<LineProxy> selectedChanged);
    }
}