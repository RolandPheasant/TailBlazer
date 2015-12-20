using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Recent;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Views
{
    public class RecentFilesViewModel:AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly IRecentFileCollection _recentFileCollection;
        private readonly IDisposable _cleanUp;
        private readonly ISubject<FileInfo> _fileOpenRequest = new Subject<FileInfo>();

        public ReadOnlyObservableCollection<RecentFileProxy> Files {get;}

        public RecentFilesViewModel(IRecentFileCollection recentFileCollection, ISchedulerProvider schedulerProvider)
        {
            _recentFileCollection = recentFileCollection;
            if (recentFileCollection == null) throw new ArgumentNullException(nameof(recentFileCollection));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));


            ReadOnlyObservableCollection<RecentFileProxy> data;
            var recentLoader = recentFileCollection.Items
                .Connect()
                .Transform(rf => new RecentFileProxy(rf, toOpen =>
                                                            {
                                                                _fileOpenRequest.OnNext(new FileInfo(toOpen.Name));
                                                            },
                                                            recentFileCollection.Remove))
                .Sort(SortExpressionComparer<RecentFileProxy>.Descending(proxy => proxy.Timestamp))
                .ObserveOn(schedulerProvider.MainThread)
                .Bind(out data)
                .Subscribe();

            Files = data;

            _cleanUp = Disposable.Create(() =>
            {
                recentLoader.Dispose();
                _fileOpenRequest.OnCompleted();
            }) ;
        }

        public IObservable<FileInfo> OpenFileRequest => _fileOpenRequest.AsObservable();

        public void Add(FileInfo fileInfo)
        {
            _recentFileCollection.Add(new RecentFile(fileInfo));
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
