using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Input;
using Dragablz;
using DynamicData.Aggregation;
using DynamicData.Binding;
using Microsoft.Win32;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;
using System.Reactive.Concurrency;

namespace TailBlazer.Views
{
    public class WindowViewModel: AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly IObjectProvider _objectProvider;
        private readonly IDisposable _cleanUp;
        private ViewContainer _selected;
        private bool _isEmpty;
        private bool _isLoading;

        public ObservableCollection<ViewContainer> Views { get; } = new ObservableCollection<ViewContainer>();
        public IInterTabClient InterTabClient { get; }
        public ICommand OpenFileCommand { get; }
        public Command ShowInGitHubCommand { get; }
        public string Version { get; }

        public FileDropMonitor DropMonitor { get; } = new FileDropMonitor();

        public WindowViewModel(IObjectProvider objectProvider, IWindowFactory windowFactory)
        {
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

            _cleanUp = Disposable.Create(() =>
                                         {
                                             isEmptyChecker.Dispose();
                                             fileDropped.Dispose();
                                             DropMonitor.Dispose();
                                             foreach (var disposable in  Views.Select(vc=>vc.Content).OfType<IDisposable>())
                                                 disposable.Dispose();
                                         });
        }
        
        public void OpenFile()
        {
            //1. open dialog to select file [get rid of this shit and create a material design file selector]
            var dialog = new OpenFileDialog {Filter = "All files (*.*)|*.*"};
            var result = dialog.ShowDialog();
            if (result != true) return;

            OpenFile( new FileInfo(dialog.FileName));
        }

        public void OpenFile(FileInfo file)
        {
            IsLoading = true;
            var scheduler = _objectProvider.Get<ISchedulerProvider>();

            scheduler.Background.Schedule(() =>
            {
                //Handle errors

                //2. resolve TailViewModel
                var factory = _objectProvider.Get<FileTailerViewModelFactory>();
                var viewModel = factory.Create(file);

                //3. Display it
                var newItem = new ViewContainer(file.Name, viewModel);

                IsLoading = false;
                //do the work on the ui thread
                scheduler.MainThread.Schedule(() =>
                {
                    Views.Add(newItem);
                    Selected = newItem;
                });
            });
        }

        public ItemActionCallback ClosingTabItemHandler => ClosingTabItemHandlerImpl;

        private void ClosingTabItemHandlerImpl(ItemActionCallbackArgs<TabablzControl> args)
        {
            var container = (ViewContainer)args.DragablzItem.DataContext;
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

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetAndRaise(ref _isLoading, value); }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
