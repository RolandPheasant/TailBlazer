using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Views.Formatting;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Tail
{
    public class TailViewModelFactory
    {
        private readonly IColourProvider _colourProvider;
        private readonly IIconProvider _iconProvider;
        private readonly IObjectProvider _objectProvider;
        private readonly ISchedulerProvider _schedulerProvider;
        private readonly ISearchMetadataFactory _searchMetadataFactory;

        public TailViewModelFactory(IObjectProvider objectProvider,
            ISchedulerProvider schedulerProvider,
            IColourProvider colourProvider,
            ISearchMetadataFactory searchMetadataFactory,
            IIconProvider iconProvider)
        {
            _objectProvider = objectProvider;
            _schedulerProvider = schedulerProvider;
            _colourProvider = colourProvider;
            _searchMetadataFactory = searchMetadataFactory;
            _iconProvider = iconProvider;
        }

        public TailViewModel Create(IEnumerable<FileInfo> filesInfo)
        {
            if (filesInfo == null) throw new ArgumentNullException(nameof(filesInfo));
            if (!filesInfo.Any()) throw new ArgumentException(nameof(filesInfo));

            var filesWatcher = _objectProvider.Get<FilesWatcher>(new[]
            {
                new ExplictArg("files", filesInfo),
                new ExplictArg("scheduler", _schedulerProvider.Background)
            });

            var searchMetadataCollection = _objectProvider.Get<ISearchMetadataCollection>();
            var searchHints = _objectProvider.Get<SearchHints>();
            var searchOptionsViewModel = new SearchOptionsViewModel(searchMetadataCollection, _searchMetadataFactory,
                _schedulerProvider, _colourProvider, _iconProvider, searchHints);

            var searchInfo = _objectProvider.Get<ISearchInfoCollection>
                (
                    new[]
                    {
                        new ExplictArg("filesWatcher", filesWatcher.List),
                        new ExplictArg("searchMetadataCollection", searchMetadataCollection)
                    }
                );

            return _objectProvider.Get<TailViewModel>(new[]
            {
                new ExplictArg("fileWatcher", filesWatcher),
                new ExplictArg("searchInfoCollection", searchInfo),
                new ExplictArg("searchMetadataCollection", searchMetadataCollection),
                new ExplictArg("searchOptionsViewModel", searchOptionsViewModel)
            });
        }

        public TailViewModel Create(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            var fileWatcher = _objectProvider.Get<IFileWatcher>(new[]
            {
                new ExplictArg("fileInfo", fileInfo),
                new ExplictArg("scheduler", _schedulerProvider.Background)
            });


            var searchMetadataCollection = _objectProvider.Get<ISearchMetadataCollection>();
            var searchHints = _objectProvider.Get<SearchHints>();
            var searchOptionsViewModel = new SearchOptionsViewModel(searchMetadataCollection, _searchMetadataFactory,
                _schedulerProvider, _colourProvider, _iconProvider, searchHints);

            var searchInfo = _objectProvider.Get<ISearchInfoCollection>
                (
                    new[]
                    {
                        new ExplictArg("filesWatcher", new[] {fileWatcher}),
                        new ExplictArg("searchMetadataCollection", searchMetadataCollection)
                    }
                );


            //I hate explicity specify named args - so fragile but hey ho.
            return _objectProvider.Get<TailViewModel>(new[]
            {
                new ExplictArg("fileWatcher", new [] { fileWatcher }),
                new ExplictArg("searchInfoCollection", searchInfo),
                new ExplictArg("searchMetadataCollection", searchMetadataCollection),
                new ExplictArg("searchOptionsViewModel", searchOptionsViewModel)
            });
        }
    }
}