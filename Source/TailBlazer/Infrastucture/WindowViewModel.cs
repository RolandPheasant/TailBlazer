using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;
using Dragablz;
using DynamicData.Binding;
using Microsoft.Win32;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Views;

namespace TailBlazer.Infrastucture
{
    public class WindowViewModel: AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly IObjectProvider _objectProvider;
        private readonly IDisposable _cleanUp;
        private ViewContainer _selected;
        
        public ObservableCollection<ViewContainer> Views { get; } = new ObservableCollection<ViewContainer>();
        public IInterTabClient InterTabClient { get; }
        public ICommand OpenFileCommand { get; }
        public Command ShowInGitHubCommand { get; }

        public FileDropMonitor DropMonitor { get; } = new FileDropMonitor();

        public WindowViewModel(IObjectProvider objectProvider, IWindowFactory windowFactory)
        {
            _objectProvider = objectProvider;
            InterTabClient = new InterTabClient(windowFactory);
            OpenFileCommand =  new Command(OpenFile);
            ShowInGitHubCommand = new Command(()=>   Process.Start("https://github.com/RolandPheasant"));

            //var menuController = Views.ToObservableChangeSet()
            //                            .Filter(vc => vc.Content is MenuItems)
            //                            .Transform(vc => (MenuItems)vc.Content)
            //                            .MergeMany(menuItem => menuItem.ItemCreated)
            //                            .Subscribe(item =>
            //                            {
            //                                Views.Add(item);
            //                                Selected = item;
            //                            });

            var fileDropped = DropMonitor.Dropped.Subscribe(OpenFile);


            _cleanUp = Disposable.Create(() =>
                                         {
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
            //2. resolve TailViewModel
            var factory = _objectProvider.Get<FileTailerViewModelFactory>();
            var viewModel = factory.Create(file);

            //3. Display it
            var newItem = new ViewContainer(file.Name, viewModel);
            Views.Add(newItem);
            Selected = newItem;

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
        
        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
