using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TailBlazer.Views
{
    public class FileDropContainer
    {
        public FileDropContainer(IEnumerable<string> files)
        {
            Files = files.Select(Path.GetFileName);
        }

        public IEnumerable<string> Files { get; }
    }
}