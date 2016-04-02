using System;
using TailBlazer.Domain.FileHandling;
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
            ILineProxyFactory lineProxyFactory)
        {
            var args = new InlineViewerArgs(lineProvider, selectedChanged, lineProxyFactory);

            return _objectProvider.Get<InlineViewer>(new ExplictArg("args", args));
        }
    }
}