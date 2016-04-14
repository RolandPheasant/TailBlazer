using System;
using System.Collections.Generic;
using System.IO;

namespace TailBlazer.Fixtures
{
    public class TestFile: IDisposable
    {
        public string FullName { get; }
        public FileInfo Info { get; }

        public TestFile()
        {

            FullName = Path.GetTempFileName();
            Info = new FileInfo(FullName);
        }

        public void Append(IEnumerable<string> lines)
        {
            File.AppendAllLines(FullName,lines);
        }

        public void Append(string line)
        {
            File.AppendAllLines(FullName, new[]{line});
        }

        public void EditText(string newText, int lineToEdit)
        {
            string[] arrLine = File.ReadAllLines(Name);
            arrLine[lineToEdit - 1] = newText;
            File.WriteAllLines(Name, arrLine);
        }

        public void Delete()
        {
            if (File.Exists(FullName))
            {
                if (Info.IsReadOnly)
                {
                    SetAttributeReadOnlyFale();
                }
                if ((Info.Attributes & FileAttributes.Hidden) != 0)
                {
                    SetAttributeSystemFalse();
                }
                File.Delete(FullName);
            }
        }

        public void Create()
        {
            File.Create(FullName);
        }

        public void Dispose()
        {
            Delete();
        }

        public void SetAttributeReadOnlyTrue()
        {
            File.SetAttributes(FullName, File.GetAttributes(FullName) | FileAttributes.ReadOnly);
        }

        private void SetAttributeReadOnlyFale()
        {
            File.SetAttributes(FullName, File.GetAttributes(FullName) & ~FileAttributes.ReadOnly);
        }

        public void SetAttributeHiddenTrue()
        {
            File.SetAttributes(FullName, File.GetAttributes(FullName) | FileAttributes.Hidden);
        }

        public void SetAttributeSystemTrue()
        {
            File.SetAttributes(FullName, File.GetAttributes(FullName) | FileAttributes.System);
        }

        public void SetAttributeSystemFalse()
        {
            File.SetAttributes(FullName, File.GetAttributes(FullName) & ~FileAttributes.System);
        }
    }
}
