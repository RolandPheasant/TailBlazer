using System;
using System.IO;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.StateHandling;
using TailBlazer.Views.Formatting;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Tail
{
    public class TailViewModelFactory
    {
        private readonly IObjectProvider _objectProvider;
        private readonly ISchedulerProvider _schedulerProvider;
        private readonly IColourProvider _colourProvider;
        private readonly ISearchMetadataFactory _searchMetadataFactory;
        private readonly IIconProvider _iconProvider;
        private readonly IStateBucketService _stateBucketService;

        public TailViewModelFactory([NotNull] IObjectProvider objectProvider,
            [NotNull] ISchedulerProvider schedulerProvider, 
            [NotNull] IColourProvider colourProvider,
            [NotNull] ISearchMetadataFactory searchMetadataFactory, 
            [NotNull] IIconProvider iconProvider,
            [NotNull] IStateBucketService stateBucketService)
        {
            if (objectProvider == null) throw new ArgumentNullException(nameof(objectProvider));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            if (colourProvider == null) throw new ArgumentNullException(nameof(colourProvider));
            if (searchMetadataFactory == null) throw new ArgumentNullException(nameof(searchMetadataFactory));
            if (iconProvider == null) throw new ArgumentNullException(nameof(iconProvider));
            if (stateBucketService == null) throw new ArgumentNullException(nameof(stateBucketService));
            _objectProvider = objectProvider;
            _schedulerProvider = schedulerProvider;
            _colourProvider = colourProvider;
            _searchMetadataFactory = searchMetadataFactory;
            _iconProvider = iconProvider;
            _stateBucketService = stateBucketService;
        }

        public TailViewModel Create(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            var fileWatcher = _objectProvider.Get<IFileWatcher>(new[]
            {
                new ExplictArg("fileInfo", fileInfo),
                new ExplictArg("scheduler",_schedulerProvider.Background)
            });


            var searchMetadataCollection = _objectProvider.Get<ISearchMetadataCollection>();
            var searchHints = _objectProvider.Get<SearchHints>();
            var searchOptionsViewModel = new SearchOptionsViewModel(searchMetadataCollection, _searchMetadataFactory, _schedulerProvider, _colourProvider, _iconProvider, searchHints);
            
            var searchInfo = _objectProvider.Get<ISearchInfoCollection>
                (
                    new[]
                    {
                        new ExplictArg("fileWatcher", fileWatcher),
                        new ExplictArg("searchMetadataCollection", searchMetadataCollection),
                    }
                );

            
            //I hate explicity specify named args - so fragile but hey ho.
            return _objectProvider.Get<TailViewModel>(new[]
            {
                new ExplictArg("fileWatcher", fileWatcher),
                new ExplictArg("searchInfoCollection", searchInfo),
                new ExplictArg("searchMetadataCollection", searchMetadataCollection),
                new ExplictArg("searchOptionsViewModel", searchOptionsViewModel)
            });
        }
    }
}