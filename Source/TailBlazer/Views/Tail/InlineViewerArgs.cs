using System;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling;

namespace TailBlazer.Views.Tail
{
    public class InlineViewerArgs
    {
        public IObservable<ILineProvider> LineProvider { get;  }
        public IObservable<LineProxy> SelectedChanged { get;  }

        public InlineViewerArgs([NotNull] IObservable<ILineProvider> lineProvider,
            [NotNull] IObservable<LineProxy> selectedChanged)
        {
            if (lineProvider == null) throw new ArgumentNullException(nameof(lineProvider));
            if (selectedChanged == null) throw new ArgumentNullException(nameof(selectedChanged));

            LineProvider = lineProvider;
            SelectedChanged = selectedChanged;
        }
    }
}