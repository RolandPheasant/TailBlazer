using System;
using System.Collections.Generic;
using Dragablz;
using TailBlazer.Domain.Infrastructure;


namespace TailBlazer.Views
{
    public class WindowFactory : IWindowFactory
    {
        private readonly IObjectProvider _objectProvider;

        public WindowFactory(IObjectProvider objectProvider)
        {
            _objectProvider = objectProvider;
        }


        public MainWindow Create(IEnumerable<string> files = null)
        {
            var window = new MainWindow();
            var model = _objectProvider.Get<WindowViewModel>();
            model.OpenFiles(files);
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