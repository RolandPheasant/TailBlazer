using System;

namespace TailBlazer.Native
{
    public interface IKeyboardNavigationHandler : IDisposable
    {
        IObservable<KeyboardNavigationType> NavigationKeys { get; }
    }
}