using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DynamicData.Kernel;
using TailBlazer.Infrastucture;

namespace TailBlazer.Native
{
    public class KeyboardNavigationHandler: IDependencyObjectReceiver, IKeyboardNavigationHandler
    {
        private readonly SerialDisposable _keySubscriber = new SerialDisposable();
        private readonly IDisposable _cleanUp;
        private readonly ISubject<KeyboardNavigationType> _keyStream = new Subject<KeyboardNavigationType>();
        
        public IObservable<KeyboardNavigationType> NavigationKeys => _keyStream.AsObservable();

        public KeyboardNavigationHandler()
        {
            _cleanUp = Disposable.Create(() =>
            {
                _keySubscriber.Dispose();
                _keyStream.OnCompleted();
            });
        }

        void IDependencyObjectReceiver.Receive(DependencyObject value)
        {
            var control = (Border) value;

            _keySubscriber.Disposable = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                                h => control.PreviewKeyDown += h, 
                                h => control.PreviewKeyDown -= h)
                .Select(e => Map(e.EventArgs))
                .Where(e => e.HasValue)
                .Select(e => e.Value)
                .SubscribeSafe(_keyStream);
        }

        private Optional<KeyboardNavigationType> Map(KeyEventArgs keyEventArgs)
        {
            switch (keyEventArgs.Key)
            {
                case Key.PageDown:
                    return KeyboardNavigationType.PageDown;

                case Key.PageUp:
                    return KeyboardNavigationType.PageUp;

                case Key.Up:
                    return KeyboardNavigationType.Up;

                case Key.Down:
                    return KeyboardNavigationType.Down;

                case Key.Home:
                    return KeyboardNavigationType.Home;
                    
                case Key.End:
                    return KeyboardNavigationType.End;

                default:
                    return Optional<KeyboardNavigationType>.None;
            }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
