using System.IO;
using DynamicData.Binding;

namespace TailBlazer.Infrastucture
{
    public class FileHeader: AbstractNotifyPropertyChanged
    {
        private readonly FileInfo _info;
        private  string _displayName;
        private bool _isPinned = false;

        public bool IsPinned {
            get { return _isPinned; }
            set { SetAndRaise(ref _isPinned, value); }
        }

        public string FullName => _info.FullName;

        public FileHeader(FileInfo info)
        {
            _info = info;
            _displayName = info.Name;
        }

        public string DisplayName
        {
            get { return _displayName; }
            set { SetAndRaise(ref _displayName,value);}
        }


    }
}