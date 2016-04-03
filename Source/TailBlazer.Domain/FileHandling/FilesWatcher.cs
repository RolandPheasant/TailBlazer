using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace TailBlazer.Domain.FileHandling
{
    public class FilesWatcher : IList<FileWatcher>
    {
        public IList<FileWatcher> List { get; } = new List<FileWatcher>();

        #region Implementation of IEnumerable
        public IEnumerator<FileWatcher> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Implementation of ICollection<T>
        public void Add(FileWatcher item)
        {
            List.Add(item);
        }

        public void Clear()
        {
            List.Clear();
        }

        public bool Contains(FileWatcher item)
        {
            return List.Contains(item);
        }

        public void CopyTo(FileWatcher[] array, int arrayIndex)
        {
            List.CopyTo(array, arrayIndex);
        }

        public bool Remove(FileWatcher item)
        {
            return List.Remove(item);
        }

        public int Count => List.Count;
        public bool IsReadOnly => List.IsReadOnly;

        public int IndexOf(FileWatcher item)
        {
            return List.IndexOf(item);
        }

        public void Insert(int index, FileWatcher item)
        {
            List.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            List.RemoveAt(index);
        }

        public FileWatcher this[int index]
        {
            get { return List[index]; }
            set { List[index] = value; }
        }
        #endregion

        public FilesWatcher(IEnumerable<FileInfo> files, IScheduler scheduler = null)
        {
            foreach (var fileInfo in files)
            {
                Add(new FileWatcher(fileInfo, scheduler));
            }
        }
    }
}