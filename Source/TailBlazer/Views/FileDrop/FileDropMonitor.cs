using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views.FileDrop
{
    public class FileDropMonitor : IDependencyObjectReceiver, IDisposable
    {
        private readonly SerialDisposable _cleanUp = new SerialDisposable();
        private readonly ISubject<FileInfo> _fileDropped = new Subject<FileInfo>();
        private bool isLoaded = false;

        public void Receive(DependencyObject value)
        {
            if (isLoaded)
                return;

            isLoaded = true;

            var control = (UIElement) value;
            control.AllowDrop = true;

            var window = Window.GetWindow(value);
            DragAdorner adorner=null;

            var createAdorner = Observable.FromEventPattern<DragEventHandler, DragEventArgs>
                (h => control.PreviewDragEnter += h, h => control.PreviewDragEnter -= h)
                .Select(ev => ev.EventArgs)
                .Subscribe(e =>
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    var container = new FileDropContainer(files);
                    var contentPresenter = new ContentPresenter {Content = container};

                    adorner = new DragAdorner(control, contentPresenter);
                });
            
            var clearAdorner = Observable.FromEventPattern<DragEventHandler, DragEventArgs>(h => control.PreviewDragLeave += h, h => control.PreviewDragLeave -= h).ToUnit()
                .Merge(Observable.FromEventPattern<DragEventHandler, DragEventArgs>(h => control.PreviewDrop += h, h => control.PreviewDrop -= h).ToUnit())
                .Subscribe(e =>
                {
                    if (adorner == null) return;
                    adorner.Detatch();

                    adorner = null;
                });



            var updatePositionOfAdornment = Observable.FromEventPattern<DragEventHandler, DragEventArgs>
                    (h => control.PreviewDragOver += h, h => control.PreviewDragOver -= h)
                    .Select(ev => ev.EventArgs)
                    .Where(_=>adorner!=null)
                    .Subscribe(e => adorner.MousePosition = e.GetPosition(window));


            var dropped = Observable.FromEventPattern<DragEventHandler, DragEventArgs>
                (h => control.Drop += h, h => control.Drop -= h)
                .Select(ev => ev.EventArgs)
                .SelectMany(e =>
                {
                    if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                        return Enumerable.Empty<FileInfo>();

                    // Note that you can have more than one file.
                    var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                    return files.Select(f => new FileInfo(f));
                })
                .SubscribeSafe(_fileDropped);


            _cleanUp.Disposable = Disposable.Create(() =>
            {
                updatePositionOfAdornment.Dispose();
                clearAdorner.Dispose();
                createAdorner.Dispose();
                dropped.Dispose();
                _fileDropped.OnCompleted();
            });
        }

        public IObservable<FileInfo> Dropped => _fileDropped;
        
        /// <summary>
        /// Taken shamelessly from https://github.com/punker76/gong-wpf-dragdrop
        /// 
        /// Thanks
        /// </summary>
        private class DragAdorner : Adorner
        {

            private readonly AdornerLayer _adornerLayer;
            private readonly UIElement _adornment;
            private Point _mousePositon;

            public DragAdorner(UIElement adornedElement,
                UIElement adornment,
                DragDropEffects effects = DragDropEffects.None)
                : base(adornedElement)
            {
                _adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
                _adornerLayer.Add(this);
                _adornment = adornment;
                IsHitTestVisible = false;
                Effects = effects;
            }

            private DragDropEffects Effects { get; set; }

            public Point MousePosition
            {
                get { return _mousePositon; }
                set
                {
                    if (_mousePositon == value) return;
                    _mousePositon = value;
                    _adornerLayer.Update(AdornedElement);
                }
            }

            public void Detatch()
            {
                _adornerLayer.Remove(this);
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                _adornment.Arrange(new Rect(finalSize));
                return finalSize;
            }

            public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
            {
                var result = new GeneralTransformGroup();
                result.Children.Add(base.GetDesiredTransform(transform));
                result.Children.Add(new TranslateTransform(MousePosition.X - 4, MousePosition.Y - 4));

                return result;
            }

            protected override Visual GetVisualChild(int index)
            {
                return _adornment;
            }

            protected override Size MeasureOverride(Size constraint)
            {
                _adornment.Measure(constraint);
                return _adornment.DesiredSize;
            }

            protected override int VisualChildrenCount => 1;

        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}