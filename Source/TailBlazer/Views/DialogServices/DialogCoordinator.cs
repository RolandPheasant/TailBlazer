using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData.Binding;

namespace TailBlazer.Views.DialogServices;

public class DialogCoordinator : IDialogCoordinator
{
    private readonly SerialDisposable _disposer = new SerialDisposable();

    public void Show(IDialogViewModel view, object content, Action<object> onClosed = null)
    {

        _disposer.Disposable = Disposable.Empty;
        view.DialogContent = content;
        view.IsDialogOpen = true;

        var closedCallback = view.WhenValueChanged(v => v.IsDialogOpen, false)
            .Where(isOpen => !isOpen)
            .Subscribe(_ => _disposer.Disposable = Disposable.Empty);

        _disposer.Disposable = Disposable.Create(() =>
        {
            closedCallback.Dispose();
            view.IsDialogOpen = false;
            onClosed?.Invoke(view.DialogContent);
        });
    }

    public void Close()
    {
        _disposer.Disposable = Disposable.Empty;
    }
}