using System;
using Dragablz;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views
{
    public class WindowFactory : IWindowFactory
    {
        private readonly IObjectProvider _objectProvider;

        public WindowFactory(IObjectProvider objectProvider)
        {
            _objectProvider = objectProvider;
        }

        public MainWindow Create()
        {
            var window = new MainWindow();
            var model = _objectProvider.Get<WindowViewModel>();
           // if (showMenu) model.ShowMenu();

            window.DataContext = model;

            window.Closing += (sender, e) =>
                              {
                                  if (TabablzControl.GetIsClosingAsPartOfDragOperation(window)) return;

                                  var todispose = ((MainWindow) sender).DataContext as IDisposable;
                                  todispose?.Dispose();
                              };

            return window;
        }
    }
}