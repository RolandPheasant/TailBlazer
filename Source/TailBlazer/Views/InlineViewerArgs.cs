using System;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling;

namespace TailBlazer.Views
{
    public class InlineViewerArgs
    {
        public IObservable<ILineProvider> LineProvider { get;  }
        public IObservable<LineProxy> SelectedChanged { get;  }
        public ILineProxyFactory LineProxyFactory { get;  }

        public InlineViewerArgs([NotNull] IObservable<ILineProvider> lineProvider,
            [NotNull] IObservable<LineProxy> selectedChanged, 
            [NotNull] ILineProxyFactory lineProxyFactory)
        {
            if (lineProvider == null) throw new ArgumentNullException(nameof(lineProvider));
            if (selectedChanged == null) throw new ArgumentNullException(nameof(selectedChanged));
            if (lineProxyFactory == null) throw new ArgumentNullException(nameof(lineProxyFactory));
            LineProvider = lineProvider;
            SelectedChanged = selectedChanged;
            LineProxyFactory = lineProxyFactory;
        }
    }
}