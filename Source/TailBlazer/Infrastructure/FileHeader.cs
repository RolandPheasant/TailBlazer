using System.IO;
using DynamicData.Binding;

namespace TailBlazer.Infrastructure;

public class FileHeader: AbstractNotifyPropertyChanged
{
    private readonly FileInfo _info;
    private  string _displayName;

    public string FullName => _info.FullName;

    public FileHeader(FileInfo info)
    {
        _info = info;
        _displayName = info.Name;
    }

    public string DisplayName
    {
        get => _displayName;
        set => SetAndRaise(ref _displayName,value);
    }


}