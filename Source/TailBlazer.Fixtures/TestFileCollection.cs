using System.Collections;
using System.Collections.Generic;

namespace TailBlazer.Fixtures
{
    public class TestFileCollection : IList<TestFile>
    {
        private readonly List<TestFile> _list;

        public TestFileCollection()
        {
            _list = new List<TestFile>();
        }

        public IEnumerator<TestFile> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TestFile item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(TestFile item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(TestFile[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(TestFile item)
        {
            return _list.Remove(item);
        }

        public int Count => _list.Count;
        public bool IsReadOnly => true;

        public int IndexOf(TestFile item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, TestFile item)
        {
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public TestFile this[int index]
        {
            get { return _list[index]; }
            set { _list[index] = value; }
        }
    }
}