using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;

namespace TailBlazer.Infrastucture
{
    public class FileDropMonitor : IDependencyObjectReceiver, IDisposable
    {
        private readonly SerialDisposable _cleanUp = new SerialDisposable();

        private readonly ISubject<FileInfo> _fileDropped = new Subject<FileInfo>();

        public void Receive(DependencyObject value)
        {
            var control = (Control)value;
            control.AllowDrop = true;


            var dragEntered = Observable.FromEventPattern<DragEventHandler, DragEventArgs>
                (h => control.DragEnter += h, h => control.DragEnter -= h)
                .Select(ev => ev.EventArgs)
                .Subscribe(e =>
                {
                    if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        e.Effects= DragDropEffects.Copy;
                    }
                });

            var dropped = Observable.FromEventPattern<DragEventHandler, DragEventArgs>
                (h => control.Drop += h, h => control.Drop -= h)
                .Select(ev => ev.EventArgs)
                .SelectMany(e =>
                {
                    if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                        return Enumerable.Empty<FileInfo>();

                    // Note that you can have more than one file.
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    return files.Select(f => new FileInfo(f));
                })
                .SubscribeSafe(_fileDropped);


            _cleanUp.Disposable = Disposable.Create(() =>
            {
                dragEntered.Dispose();
                dropped.Dispose();
                _fileDropped.OnCompleted();
            });

        }

        public IObservable<FileInfo> Dropped => _fileDropped;

        public void ScrollToEnd()
        {

        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}