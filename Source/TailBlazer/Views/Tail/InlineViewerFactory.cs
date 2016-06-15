using System;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Views.Tail
{
    public class InlineViewerFactory : IInlineViewerFactory
    {
        private readonly IObjectProvider _objectProvider;

        public InlineViewerFactory(IObjectProvider objectProvider)
        {
            _objectProvider = objectProvider;
        }

        public InlineViewer Create(ICombinedSearchMetadataCollection combinedSearchMetadataCollection,  
            IObservable<ILineProvider> lineProvider, 
            IObservable<LineProxy> selectedChanged)
        {
            var args = new IArgument[]
            {
                new Argument<IObservable<ILineProvider>>(lineProvider),
                new Argument<IObservable<LineProxy>>(selectedChanged),
                new Argument<ICombinedSearchMetadataCollection>(combinedSearchMetadataCollection)
            };
            return _objectProvider.Get<InlineViewer>(args);
        }
    }
}