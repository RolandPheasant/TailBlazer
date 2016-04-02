using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Binding;

namespace TailBlazer.Infrastucture
{
    public class FilesHeader : AbstractNotifyPropertyChanged
    {
        private string _displayName;

        public string FullName { get; }

        public FilesHeader(IEnumerable<FileInfo> info)
        {
            _displayName = "Tailed files ("+info.Count()+")";
            foreach (var fileInfo in info)
            {
                FullName += fileInfo.FullName + Environment.NewLine;
            }
        }

        public string DisplayName
        {
            get { return _displayName; }
            set { SetAndRaise(ref _displayName, value); }
        }
    }
}
