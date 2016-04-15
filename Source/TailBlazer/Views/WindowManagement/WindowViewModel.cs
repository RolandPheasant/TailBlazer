using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dragablz;
using DynamicData;
using DynamicData.Binding;
using MaterialDesignThemes.Wpf;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;
using TailBlazer.Views.FileDrop;
using TailBlazer.Views.FileOpen;
using TailBlazer.Views.Layout;
using TailBlazer.Views.Options;
using TailBlazer.Views.Recent;
using TailBlazer.Views.Tail;
using TailBlazer.Views.Dialog;
using MaterialDesignThemes.Wpf;
using Microsoft.Expression.Interactivity.Core;

namespace TailBlazer.Views.WindowManagement
{
    public class WindowViewModel : AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IWindowsController _windowsController;
        private readonly ISchedulerProvider _schedulerProvider;
        private readonly IObjectProvider _objectProvider;
        private readonly IDisposable _cleanUp;
        private ViewContainer _selected;
        private bool _isEmpty;
        private bool _menuIsOpen;
        public ICommand Pinning { get; set; }
        public ObservableCollection<ViewContainer> Views { get; } = new ObservableCollection<ViewContainer>();
        public RecentFilesViewModel RecentFiles { get; }
        public GeneralOptionsViewModel GeneralOptions { get; }
        public IInterTabClient InterTabClient { get; }
        public FileOpenViewModel FileOpen { get; }
        public ICommand FileOpenDialogCommand { get; }

        public Command ShowInGitHubCommand { get; }
        public string Version { get; }
        public ICommand SaveLayoutCommand { get; }
        public ICommand ExitCommmand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }

        public FileDropMonitor DropMonitor { get; } = new FileDropMonitor();

        public static int OpenedFileCount { get; set; }

        public DialogViewModel Dialog { get; set; }

        public WindowViewModel(IObjectProvider objectProvider,
            IWindowFactory windowFactory,
            ILogger logger,
            IWindowsController windowsController,
            RecentFilesViewModel recentFilesViewModel,
            GeneralOptionsViewModel generalOptionsViewModel,
            ISchedulerProvider schedulerProvider,
            DialogViewModel dialogviewmodel,
            FileOpenViewModel fileopenviewmodel)
        {
            Pinning = new ActionCommand(o =>
            {
                var content = o as string;

            });
            _logger = logger;
            _windowsController = windowsController;
            RecentFiles = recentFilesViewModel;
            GeneralOptions = generalOptionsViewModel;
            Dialog = dialogviewmodel;
            _schedulerProvider = schedulerProvider;
            _objectProvider = objectProvider;
            InterTabClient = new InterTabClient(windowFactory);
            FileOpenDialogCommand = new Command(OpenFileDialog);
            FileOpen = new FileOpenViewModel(OpenFile);

            ShowInGitHubCommand = new Command(() => Process.Start("https://github.com/RolandPheasant"));

            Views.CollectionChanged += Views_CollectionChanged;

            ZoomOutCommand = new Command(() => { GeneralOptions.Scale = GeneralOptions.Scale + 5; });
            ZoomInCommand = new Command(() => { GeneralOptions.Scale = GeneralOptions.Scale - 5; });
            SaveLayoutCommand = new Command(WalkTheLayout);
            ExitCommmand = new Command(() => Application.Current.Shutdown());

            Version = $"v{Assembly.GetEntryAssembly().GetName().Version.ToString(3)}";

            var fileDropped = DropMonitor.Dropped.Subscribe(async t => await OpenFile(t));
            var isEmptyChecker = Views.ToObservableChangeSet()
                                    .ToCollection()
                                    .Select(items => items.Count)
                                    .StartWith(0)
                                    .Select(count => count == 0)
                                    .Subscribe(isEmpty => IsEmpty = isEmpty);

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
                        .ForEach(d => d.Dispose());
                }));
        }

        private async void OpenFileDialog()
        {
            await DialogHost.Show(FileOpen, DialogNames.EntireWindow);
        }

        private void WalkTheLayout()
        {
            var analyser = new LayoutAnalyser();
            var root = analyser.QueryLayouts();
            Console.WriteLine(root);
        }

        public void OpenFiles(IEnumerable<string> files = null)
        {
            if (files == null) return;

            foreach (var file in files)
                OpenFile(new[] { new FileInfo(file) });
        }

        private async Task<bool> OpenFile(IEnumerable<FileInfo> files)
        {
            OpenedFileCount = files.Count();
            if (OpenedFileCount > 1)
            {
                //Here we can set the dialog window's message
                Dialog.text = "Would you like to tail these files?";
                //Showing the dialog window
                await DialogHost.Show(Dialog, FileOpen.Id);

                //Testing the pushed button
                if (Dialog.Button)
                {
                    //Tailing multiple files
                    _schedulerProvider.Background.Schedule(() =>
                    {
                        //await DialogHost.Show(Dialog, DialogNames.EntireWindow);
                        try
                        {
                            _logger.Info($"Attempting to open '{files.Count()}' files");

                            var factory = _objectProvider.Get<TailViewModelFactory>();
                            var viewModel = factory.Create(files);

                            var newItem = new ViewContainer(new FilesHeader(files), viewModel);

                            _windowsController.Register(newItem);

                            //_logger.Info($"Objects for '{file.FullName}' has been created.");
                            //do the work on the ui thread
                            _schedulerProvider.MainThread.Schedule(() =>
                            {

                                Views.Add(newItem);
                                _logger.Info($"Opened '{files.Count()}' files");
                                Selected = newItem;
                            });
                        }
                        catch
                        {
                            // ignored
                        }

                    });
                }
                else
                {
                    foreach (var fileInfo in files)
                    {
                        OpenFile(fileInfo);
                    }
                }
            }
            else
            {
                OpenFile(files.ElementAt(0));
            }

            return true;
        }

        private void Views_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //throw new NotImplementedException();
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
                    var factory = _objectProvider.Get<TailViewModelFactory>();
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
