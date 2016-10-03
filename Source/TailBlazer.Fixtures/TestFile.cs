using System;
using System.Collections.Generic;
using System.IO;

namespace TailBlazer.Fixtures
{
    public class TestFile: IDisposable
    {
        public string Name { get; }
        public FileInfo Info => new FileInfo(Name);

        public TestFile(string name = null)
        {
            if (name == null)
            {
                var filename = string.Format(@"{0}.txt", Guid.NewGuid());
                var path = Path.GetTempPath();
                var fullPath = Path.Combine(path, filename);
                Name = fullPath;
            }
            else
            {
                var path = Path.GetTempPath();
                var fullPath = Path.Combine(path, name);
                if (File.Exists(fullPath)) File.Delete(fullPath);
                Name = fullPath;
            }
        }

        public void Append(IEnumerable<string> lines)
        {
            File.AppendAllLines(Name,lines);
        }

        public void Append(string line)
        {
            File.AppendAllLines(Name, new[]{line});
        }

        public void Delete()
        {
            File.Delete(Name);
        }

        public void Create()
        {
            File.Create(Name);
        }

        public void Dispose()
        {
            File.Delete(Name);
        }
    }
}
