using System;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Views.Tail
{
    public interface IInlineViewerFactory
    {
        InlineViewer Create(IObservable<ILineProvider> lineProvider,IObservable<LineProxy> selectedChanged, ISearchMetadataCollection searchMetadataCollection);
    }
}