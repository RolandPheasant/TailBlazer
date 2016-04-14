using System.IO;
using System.Windows.Media;

namespace TailBlazer.Views.FileOpen
{
    /// <summary>
    /// Stores a file's FileInfo and Icon
    /// </summary>
    public class FileInfoWithIcon
    {
        public FileInfo FileInfo { get; set; }
        public ImageSource ImageSource { get; set; }
    }
}
