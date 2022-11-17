using System;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Views.Tail;

public interface IInlineViewerFactory
{
    InlineViewer Create(ICombinedSearchMetadataCollection combinedSearchMetadataCollection, IObservable<ILineProvider> lineProvider,IObservable<LineProxy> selectedChanged);
}