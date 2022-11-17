using System.ComponentModel;

namespace TailBlazer.Views.DialogServices;

public interface IDialogViewModel : INotifyPropertyChanged
{
    bool IsDialogOpen { get; set; }
    object DialogContent { get; set; }
}