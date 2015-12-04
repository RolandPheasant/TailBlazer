using System;
using System.Collections.Generic;
using System.IO;

namespace TailBlazer.Fixtures
{
    public class TestFile: IDisposable
    {
        public string Name { get; }
        public FileInfo Info { get; }

        public TestFile()
        {
            Name = Path.GetTempFileName();
            Info = new FileInfo(Name);
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

        public void Dispose()
        {
            File.Delete(Name);
        }
    }
}
