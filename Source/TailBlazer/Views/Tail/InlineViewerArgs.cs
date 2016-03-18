using System;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Views.Tail
{
    public class InlineViewerArgs
    {
        public IObservable<ILineProvider> LineProvider { get;  }
        public IObservable<LineProxy> SelectedChanged { get;  }
        public ISearchMetadataCollection SearchMetadataCollection { get;  }

        public InlineViewerArgs([NotNull] IObservable<ILineProvider> lineProvider,
            [NotNull] IObservable<LineProxy> selectedChanged,
            [NotNull] ISearchMetadataCollection searchMetadataCollection)
        {
            if (lineProvider == null) throw new ArgumentNullException(nameof(lineProvider));
            if (selectedChanged == null) throw new ArgumentNullException(nameof(selectedChanged));
            if (searchMetadataCollection == null) throw new ArgumentNullException(nameof(searchMetadataCollection));

            LineProvider = lineProvider;
            SelectedChanged = selectedChanged;
            SearchMetadataCollection = searchMetadataCollection;
        }
    }
}