using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Input;
using Dragablz;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;
using Microsoft.Win32;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;
using System.Reactive.Concurrency;
using TailBlazer.Domain.FileHandling;

namespace TailBlazer.Views
{
    public class WindowViewModel: AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IWindowsController _windowsController;
        private readonly IRecentFiles _recentFiles;
        private readonly ISchedulerProvider _schedulerProvider;
        private readonly IObjectProvider _objectProvider;
        private readonly IDisposable _cleanUp;
        private ViewContainer _selected;
        private bool _isEmpty;

        public ObservableCollection<ViewContainer> Views { get; } = new ObservableCollection<ViewContainer>();

        public ReadOnlyObservableCollection<FileInfo> RecentFiles { get; } 

        public IInterTabClient InterTabClient { get; }
        public ICommand OpenFileCommand { get; }
        public Command ShowInGitHubCommand { get; }
        public string Version { get; }

        public FileDropMonitor DropMonitor { get; } = new FileDropMonitor();

        public WindowViewModel(IObjectProvider objectProvider, 
            IWindowFactory windowFactory, 
            ILogger logger,
            IWindowsController windowsController,
            IRecentFiles  recentFiles,
            ISchedulerProvider schedulerProvider )
        {
            _logger = logger;
            _windowsController = windowsController;
            _recentFiles = recentFiles;
            _schedulerProvider = schedulerProvider;
            _objectProvider = objectProvider;
            InterTabClient = new InterTabClient(windowFactory);
            OpenFileCommand =  new Command(OpenFile);
            ShowInGitHubCommand = new Command(()=>   Process.Start("https://github.com/RolandPheasant"));


            Version = $"v{Assembly.GetEntryAssembly().GetName().Version.ToString(3)}";

            var fileDropped = DropMonitor.Dropped.Subscribe(OpenFile);
            var isEmptyChecker = Views.ToObservableChangeSet()
                                    .Count()
                                    .StartWith(0)
                                    .Select(count=>count==0)
                                    .Subscribe(isEmpty=> IsEmpty = isEmpty);



            //move this out into it's own view model + create proxy so we can order
            //and timestamp etc
            //ReadOnlyObservableCollection<FileInfo> data;
            //var recentLoader = recentFiles.Items
            //                    .Connect()
            //                    .ObserveOn(schedulerProvider.MainThread)
            //                    .Bind(out data)
            //                    .Subscribe();
          //  RecentFiles = data;

            _cleanUp = new CompositeDisposable(//recentLoader,
                isEmptyChecker,
                fileDropped,
                DropMonitor,
                Disposable.Create(() =>
                {
                     Views.Select(vc => vc.Content)
                            .OfType<IDisposable>()
                            .ForEach(d=>d.Dispose());
                }));

        }
        
        public void OpenFile()
        {
            // open dialog to select file [get rid of this shit and create a material design file selector]
            var dialog = new OpenFileDialog {Filter = "All files (*.*)|*.*"};
            var result = dialog.ShowDialog();
            if (result != true) return;

            OpenFile(new FileInfo(dialog.FileName));
        }

        public void OpenFile(FileInfo file)
        {
          // var scheduler = _objectProvider.Get<ISchedulerProvider>();

            _schedulerProvider.Background.Schedule(() =>
            {
                //Handle errors
                try
                {
                    _logger.Info($"Attempting to open '{file.FullName}'");

                    _recentFiles.Register(file);

                    //1. resolve TailViewModel
                    var factory = _objectProvider.Get<FileTailerViewModelFactory>();
                    var viewModel = factory.Create(file);

                    //2. Display it
                    var newItem = new ViewContainer(new FileHeader(file), viewModel);
                    
                    _windowsController.Register(newItem);


                    //do the work on the ui thread
                    _schedulerProvider.MainThread.Schedule(() =>
                    {
                        Views.Add(newItem);
                        _logger.Info($"Opened '{file.FullName}'");
                        Selected = newItem;
                    });
                }
                catch (Exception ex)
                {
                    //TODO: Create a failed to load view
                    _logger.Error(ex, $"There was a problem opening '{file.FullName}'");

                }
            });
        }

        public ItemActionCallback ClosingTabItemHandler => ClosingTabItemHandlerImpl;

        private void ClosingTabItemHandlerImpl(ItemActionCallbackArgs<TabablzControl> args)
        {
           
            var container = (ViewContainer)args.DragablzItem.DataContext;
            _windowsController.Remove(container);
            if (container.Equals(Selected))
            {
                Selected = Views.FirstOrDefault(vc => vc != container);
            }
            var disposable = container.Content as IDisposable;
            disposable?.Dispose();
        }

        public ViewContainer Selected
        {
            get { return _selected; }
            set { SetAndRaise(ref _selected, value); }
        }

        public bool IsEmpty
        {
            get { return _isEmpty; }
            set { SetAndRaise(ref _isEmpty, value); }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
