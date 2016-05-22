using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TailBlazer.Views.FileDrop
{
    public class FileDropContainer
    {
        public FileDropContainer(IEnumerable<string> files)
        {
            if (null == files)
            {
                Files = new string[0];
                return;
            }

            Files = files
                .Where(x => null != x)
                .Select(Path.GetFileName)
                .ToArray();
        }

        public IEnumerable<string> Files { get; }
    }
}