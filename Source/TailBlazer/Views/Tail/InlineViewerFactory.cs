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

        public InlineViewer Create(IObservable<ILineProvider> lineProvider,
            IObservable<LineProxy> selectedChanged, 
            ISearchMetadataCollection searchMetadataCollection)
        {
            
            var args = new InlineViewerArgs(lineProvider, selectedChanged, searchMetadataCollection);

            return _objectProvider.Get<InlineViewer>(new NamedArgument("args", args));
        }
    }
}