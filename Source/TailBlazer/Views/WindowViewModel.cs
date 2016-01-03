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
using DynamicData.Binding;
using Microsoft.Win32;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;
using System.Reactive.Concurrency;
using DynamicData;
using TailBlazer.Settings;

namespace TailBlazer.Views
{
    public class WindowViewModel: AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IWindowsController _windowsController;
        private readonly ISchedulerProvider _schedulerProvider;
        private readonly IObjectProvider _objectProvider;
        private readonly IDisposable _cleanUp;
        private ViewContainer _selected;
        private bool _isEmpty;
        private bool _menuIsOpen;
        public ObservableCollection<ViewContainer> Views { get; } = new ObservableCollection<ViewContainer>();
        public RecentFilesViewModel RecentFiles { get; }
        public GeneralOptionsViewModel GeneralOptions { get; }
        public IInterTabClient InterTabClient { get; }
        public ICommand OpenFileCommand { get; }
        public Command ShowInGitHubCommand { get; }
        public string Version { get; }

        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        
        public FileDropMonitor DropMonitor { get; } = new FileDropMonitor();

        public WindowViewModel(IObjectProvider objectProvider, 
            IWindowFactory windowFactory, 
            ILogger logger,
            IWindowsController windowsController,
            RecentFilesViewModel recentFilesViewModel,
            GeneralOptionsViewModel generalOptionsViewModel,
            ISchedulerProvider schedulerProvider)
        {
            _logger = logger;
            _windowsController = windowsController;
            RecentFiles = recentFilesViewModel;
            GeneralOptions = generalOptionsViewModel;
            _schedulerProvider = schedulerProvider;
            _objectProvider = objectProvider;
            InterTabClient = new InterTabClient(windowFactory);
            OpenFileCommand =  new Command(OpenFile);
            ShowInGitHubCommand = new Command(()=>   Process.Start("https://github.com/RolandPheasant"));

            ZoomOutCommand= new Command(()=> { GeneralOptions.Scale = GeneralOptions.Scale + 5; });
            ZoomInCommand = new Command(() => { GeneralOptions.Scale = GeneralOptions.Scale - 5; });

            Version = $"v{Assembly.GetEntryAssembly().GetName().Version.ToString(3)}";

            var fileDropped = DropMonitor.Dropped.Subscribe(OpenFile);
            var isEmptyChecker = Views.ToObservableChangeSet()
                                    .ToCollection()
                                    .Select(items=>items.Count)
                                    .StartWith(0)
                                    .Select(count=>count==0)
                                    .Subscribe(isEmpty=> IsEmpty = isEmpty);

            var openRecent = recentFilesViewModel.OpenFileRequest
                                .Subscribe(file =>
                                {
                                    MenuIsOpen = false;
                                    OpenFile(file);
                                });
            
            _cleanUp = new CompositeDisposable(recentFilesViewModel,
                isEmptyChecker,
                fileDropped,
                DropMonitor,
                openRecent,
                Disposable.Create(() =>
                {
                     Views.Select(vc => vc.Content)
                            .OfType<IDisposable>()
                            .ForEach(d=>d.Dispose());
                }));
        }

        private void OpenFile()
        {
            // open dialog to select file [get rid of this shit and create a material design file selector]
            var dialog = new OpenFileDialog {Filter = "All files (*.*)|*.*"};
            var result = dialog.ShowDialog();
            if (result != true) return;

            OpenFile(new FileInfo(dialog.FileName));
        }

        private void OpenFile(FileInfo file)
        {
            _schedulerProvider.Background.Schedule(() =>
            {
                try
                {
                    _logger.Info($"Attempting to open '{file.FullName}'");

                    RecentFiles.Add(file);

                    //1. resolve TailViewModel
                    var factory = _objectProvider.Get<FileTailerViewModelFactory>();
                    var viewModel = factory.Create(file);

                    //2. Display it
                    var newItem = new ViewContainer(new FileHeader(file), viewModel);
                    
                    _windowsController.Register(newItem);

                    _logger.Info($"Objects for '{file.FullName}' has been created.");
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

        public void OnWindowClosing()
        {
            _logger.Info("Window is closing. {0} view to close", Views.Count);
            Views.ForEach(v => _windowsController.Remove(v));

            Views.Select(vc => vc.Content)
                .OfType<IDisposable>()
                .ForEach(x => x.Dispose());
        }

        private void ClosingTabItemHandlerImpl(ItemActionCallbackArgs<TabablzControl> args)
        {
            _logger.Info("Tab is closing. {0} view to close", Views.Count);
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

        public bool MenuIsOpen
        {
            get { return _menuIsOpen; }
            set { SetAndRaise(ref _menuIsOpen, value); }
        }


        public void Dispose()
        {
            _cleanUp.Dispose();
        }


    }
}
