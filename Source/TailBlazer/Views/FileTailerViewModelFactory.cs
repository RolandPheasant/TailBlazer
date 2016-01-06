using System;
using System.IO;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Settings;

namespace TailBlazer.Views
{
    public class FileTailerViewModelFactory
    {
        private readonly IObjectProvider _objectProvider;
        private readonly ISchedulerProvider _schedulerProvider;

        public FileTailerViewModelFactory(IObjectProvider objectProvider, ISchedulerProvider schedulerProvider)
        {
            _objectProvider = objectProvider;
            _schedulerProvider = schedulerProvider;
        }

        public FileTailerViewModel Create(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            var fileWatcher = _objectProvider.Get<IFileWatcher>(new[]
            {
                new ExplictArg("fileInfo", fileInfo),
                new ExplictArg("scheduler",_schedulerProvider.Background)
            });


            var searchMetadataCollection = _objectProvider.Get<ISearchMetadataCollection>();
            var searchOptionsViewModel = new SearchOptionsViewModel(searchMetadataCollection, _schedulerProvider);


            var searchInfo = _objectProvider.Get<ISearchInfoCollection>
                (new[]
                    {
                        new ExplictArg("fileWatcher", fileWatcher),
                        new ExplictArg("searchMetadataCollection", searchMetadataCollection),
                
                    }
                );



            //I hate explicity specify named args - so fragile but hey ho.
            return _objectProvider.Get<FileTailerViewModel>(new[] 
            {
                new ExplictArg("fileWatcher", fileWatcher),
                new ExplictArg("searchInfoCollection", searchInfo),
                new ExplictArg("searchMetadataCollection", searchMetadataCollection),
               new ExplictArg("searchOptionsViewModel", searchOptionsViewModel)
            });
        }
    }
}