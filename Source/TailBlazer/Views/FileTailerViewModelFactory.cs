using System;
using System.IO;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Views
{
    public class FileTailerViewModelFactory
    {
        private readonly IObjectProvider _objectProvider;

        public FileTailerViewModelFactory(IObjectProvider objectProvider)
        {
            _objectProvider = objectProvider;
        }

        public FileTailerViewModel Create(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
            //I hate explicity specify named args - so fragile but hey ho.
            return _objectProvider.Get<FileTailerViewModel>(new ExplictArg("fileInfo", fileInfo));
        }
    }
}