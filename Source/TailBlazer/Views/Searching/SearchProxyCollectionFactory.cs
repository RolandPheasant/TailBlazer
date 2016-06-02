using System;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.FileHandling.TextAssociations;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Views.Formatting;

namespace TailBlazer.Views.Searching
{
    public class SearchProxyCollectionFactory : ISearchProxyCollectionFactory
    {
        private ISearchMetadataFactory SearchMetadataFactory { get;  }
        private ISchedulerProvider SchedulerProvider { get;  }
        private IColourProvider ColourProvider { get;  }
        private IIconProvider IconsProvider { get;  }
        private ITextAssociationCollection TextAssociationCollection { get;  }
        private IThemeProvider ThemeProvider { get;  }

        public SearchProxyCollectionFactory([NotNull] ISearchMetadataFactory searchMetadataFactory,
            [NotNull] ISchedulerProvider schedulerProvider,
            [NotNull] IColourProvider colourProvider,
            [NotNull] IIconProvider iconsProvider,
            [NotNull] ITextAssociationCollection textAssociationCollection,
            [NotNull] IThemeProvider themeProvider)
        {
            if (searchMetadataFactory == null) throw new ArgumentNullException(nameof(searchMetadataFactory));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            if (colourProvider == null) throw new ArgumentNullException(nameof(colourProvider));
            if (iconsProvider == null) throw new ArgumentNullException(nameof(iconsProvider));
            if (textAssociationCollection == null) throw new ArgumentNullException(nameof(textAssociationCollection));
            if (themeProvider == null) throw new ArgumentNullException(nameof(themeProvider));
            SearchMetadataFactory = searchMetadataFactory;
            SchedulerProvider = schedulerProvider;
            ColourProvider = colourProvider;
            IconsProvider = iconsProvider;
            TextAssociationCollection = textAssociationCollection;
            ThemeProvider = themeProvider;
        }
        public ISearchProxyCollection Create([NotNull] ISearchMetadataCollection metadataCollection, Guid id)
        {
            if (metadataCollection == null) throw new ArgumentNullException(nameof(metadataCollection));

            return new SearchProxyCollection(metadataCollection, id, SchedulerProvider, ColourProvider, IconsProvider, TextAssociationCollection, ThemeProvider);
        }
    }
}