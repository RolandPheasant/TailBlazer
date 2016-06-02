using System;
using System.IO;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Infrastucture;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Tail
{
    public class TailViewModelFactory: IViewModelFactory
    {
        private readonly IObjectProvider _objectProvider;
        private readonly ISchedulerProvider _schedulerProvider;
        private readonly ISearchMetadataFactory _searchMetadataFactory;
        private readonly ITailViewStateControllerFactory _tailViewStateControllerFactory;
        private readonly IGlobalSearchOptions _globalSearchOptions;
        private readonly ISearchProxyCollectionFactory _searchProxyCollectionFactory;

        public TailViewModelFactory([NotNull] IObjectProvider objectProvider,
            [NotNull] ISchedulerProvider schedulerProvider, 
            [NotNull] ISearchMetadataFactory searchMetadataFactory, 
            [NotNull] ITailViewStateControllerFactory tailViewStateControllerFactory,
            [NotNull] IGlobalSearchOptions globalSearchOptions,
            [NotNull] ISearchProxyCollectionFactory searchProxyCollectionFactory) 
        {
            if (objectProvider == null) throw new ArgumentNullException(nameof(objectProvider));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            if (searchMetadataFactory == null) throw new ArgumentNullException(nameof(searchMetadataFactory));
            if (tailViewStateControllerFactory == null) throw new ArgumentNullException(nameof(tailViewStateControllerFactory));
            if (globalSearchOptions == null) throw new ArgumentNullException(nameof(globalSearchOptions));
            if (searchProxyCollectionFactory == null) throw new ArgumentNullException(nameof(searchProxyCollectionFactory));

            _objectProvider = objectProvider;
            _schedulerProvider = schedulerProvider;
            _searchMetadataFactory = searchMetadataFactory;
            _tailViewStateControllerFactory = tailViewStateControllerFactory;
            _globalSearchOptions = globalSearchOptions;
            _searchProxyCollectionFactory = searchProxyCollectionFactory;
        }
        
        public HeaderedView Create(ViewState state)
        {
            var converter = new TailViewToStateConverter();
            var converted = converter.Convert(state.State);

            var file = converted.FileName;
            var viewModel = CreateView(new FileInfo(file));

            var restorer = (IPersistentView)viewModel;
            restorer.Restore(state);
            return new HeaderedView(new FileHeader(new FileInfo(file)), viewModel);
        }

        public HeaderedView Create(FileInfo fileInfo)
        {
            var viewModel = CreateView(fileInfo);
            viewModel.ApplySettings();//apply default values
            return new HeaderedView(new FileHeader(fileInfo), viewModel);
        }

        private TailViewModel CreateView(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            var fileWatcher = _objectProvider.Get<IFileWatcher>(new[]
            {
                new ExplictArg("fileInfo", fileInfo),
                new ExplictArg("scheduler",_schedulerProvider.Background)
            });
            
            var searchMetadataCollection = _objectProvider.Get<ISearchMetadataCollection>();
            var searchHints = _objectProvider.Get<SearchHints>();
            var searchOptionsViewModel = new SearchOptionsViewModel(searchMetadataCollection, _globalSearchOptions, _searchProxyCollectionFactory, _searchMetadataFactory, _schedulerProvider, searchHints);

            var searchInfo = _objectProvider.Get<ISearchInfoCollection>
                (
                    new[]
                    {
                        new ExplictArg("fileWatcher", fileWatcher),
                        new ExplictArg("searchMetadataCollection", searchMetadataCollection)
                    }
                );
            
            //I hate explicity specify named args - so fragile but hey ho.
            var viewModel = _objectProvider.Get<TailViewModel>(new[]
            {
                new ExplictArg("fileWatcher", fileWatcher),
                new ExplictArg("searchInfoCollection", searchInfo),
                new ExplictArg("searchMetadataCollection", searchMetadataCollection),
                new ExplictArg("searchOptionsViewModel", searchOptionsViewModel),
                new ExplictArg("tailViewStateControllerFactory", _tailViewStateControllerFactory)
            });

            return viewModel;
        }
        
        public string Key => TailViewModelConstants.ViewKey;
    }
}