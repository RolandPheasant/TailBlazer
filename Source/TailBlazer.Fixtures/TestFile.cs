using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
                Name = Name = Path.GetTempFileName(); ;
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

            File.AppendAllLines(Name, lines);
            //using (var fs = new FileStream(Name, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            //{

            //    using (var writer = TextWriter)
            //    {
                    
            //    }

            //    byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine)
            //    fs.Write(b, 0, b.Length);
            //    byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
            //    fs.Write(newline, 0, newline.Length);

            //}
           
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
