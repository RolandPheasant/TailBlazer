using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DynamicData.Kernel;

namespace TailBlazer.Infrastructure.KeyboardNavigation;

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
        var control = (Control) value;

        control.IsKeyboardFocusedChanged += Control_IsKeyboardFocusedChanged;

        _keySubscriber.Disposable = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                h => control.PreviewKeyDown += h, 
                h => control.PreviewKeyDown -= h)
            .Select(e => Map(e.EventArgs))
            .Where(e => e.HasValue)
            .Select(e => e.Value)
            .SubscribeSafe(_keyStream);
    }

    private void Control_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        //   Console.WriteLine(e.NewValue);
        // var focused = (Boolean) e.NewValue;
        //if (!focused)
        //{
        //    IInputElement focusedControl = Keyboard.FocusedElement;
        //    Console.WriteLine(focusedControl.GetType());

        //  //  ((Control) focusedControl).Background =new SolidColorBrush(Colors.PaleVioletRed);;
        //}
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