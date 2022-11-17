using System;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.FileHandling.TextAssociations;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Views.Formatting;

namespace TailBlazer.Views.Searching;

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
        SearchMetadataFactory = searchMetadataFactory ?? throw new ArgumentNullException(nameof(searchMetadataFactory));
        SchedulerProvider = schedulerProvider ?? throw new ArgumentNullException(nameof(schedulerProvider));
        ColourProvider = colourProvider ?? throw new ArgumentNullException(nameof(colourProvider));
        IconsProvider = iconsProvider ?? throw new ArgumentNullException(nameof(iconsProvider));
        TextAssociationCollection = textAssociationCollection ?? throw new ArgumentNullException(nameof(textAssociationCollection));
        ThemeProvider = themeProvider ?? throw new ArgumentNullException(nameof(themeProvider));
    }
    public ISearchProxyCollection Create([NotNull] ISearchMetadataCollection metadataCollection, Guid id, Action<SearchMetadata> changeScopeAction)
    {
        if (metadataCollection == null) throw new ArgumentNullException(nameof(metadataCollection));

        return new SearchProxyCollection(metadataCollection, id,changeScopeAction, SchedulerProvider, ColourProvider, IconsProvider, TextAssociationCollection, ThemeProvider);
    }
}