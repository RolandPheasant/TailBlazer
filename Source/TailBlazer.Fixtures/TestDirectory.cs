using System;
using System.Collections.Generic;
using System.IO;

namespace TailBlazer.Fixtures
{
    public class TestDirectory : IDisposable
    {
        public string FullName { get; }
        public DirectoryInfo Info { get; }

        public TestDirectory()
        {
            
            FullName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            while (Directory.Exists(FullName))
            {
                FullName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }

            Directory.CreateDirectory(FullName);
            Info = new DirectoryInfo(FullName);
        }

        public void CopyTestFileToDirectory(TestFile testFile)
        {
            File.Copy(Path.Combine(testFile.Info.Directory.FullName, testFile.Info.Name), Path.Combine(FullName, testFile.Info.Name));
        }

        public FileInfo[] GetFiles()
        {
            return new DirectoryInfo(FullName).GetFiles();
        }

        public void Delete()
        {
            if (Directory.Exists(FullName))
            { 
                Directory.Delete(FullName, true);
            }
        }

        public void Create()
        {
            Directory.CreateDirectory(FullName);
        }

        public void Dispose()
        {
           Delete();
        }
    }
}
