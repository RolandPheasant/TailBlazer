using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using DynamicData.Binding;

namespace TailBlazer.Infrastucture
{

    public class VirtualisingCollection<T> : AbstractNotifyPropertyChanged, IList<T>, IList, INotifyCollectionChanged
    {
        private readonly IVirtualController<T> _controller;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public VirtualisingCollection(IVirtualController<T> controller)
        {
            _controller = controller;
            controller.ItemsAdded.Subscribe(x =>
            {
            });
        }


        #region Paging


        ///// <summary>
        ///// Cleans up any stale pages that have not been accessed in the period dictated by PageTimeout.
        ///// </summary>
        //public void CleanUpPages()
        //{
        //    List<int> keys = new List<int>(_pageTouchTimes.Keys);
        //    foreach (int key in keys)
        //    {
        //        // page 0 is a special case, since WPF ItemsControl access the first item frequently
        //        if (key != 0 && (DateTime.Now - _pageTouchTimes[key]).TotalMilliseconds > PageTimeout)
        //        {
        //            _pages.Remove(key);
        //            _pageTouchTimes.Remove(key);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Populates the page within the dictionary.
        ///// </summary>
        ///// <param name="pageIndex">Index of the page.</param>
        ///// <param name="page">The page.</param>
        //protected virtual void PopulatePage(int pageIndex, IList<T> page)
        //{
        //    if (_pages.ContainsKey(pageIndex))
        //        _pages[pageIndex] = page;
        //}

        ///// <summary>
        ///// Makes a request for the specified page, creating the necessary slots in the dictionary,
        ///// and updating the page touch time.
        ///// </summary>
        ///// <param name="pageIndex">Index of the page.</param>
        //protected virtual void RequestPage(int pageIndex)
        //{
        //    if (!_pages.ContainsKey(pageIndex))
        //    {
        //        _pages.Add(pageIndex, null);
        //        _pageTouchTimes.Add(pageIndex, DateTime.Now);
        //        LoadPage(pageIndex);
        //    }
        //    else
        //    {
        //        _pageTouchTimes[pageIndex] = DateTime.Now;
        //    }
        //}

        ///// <summary>
        ///// Loads the count of items.
        ///// </summary>
        //protected virtual void LoadCount()
        //{
        //    Count = FetchCount();
        //}

        ///// <summary>
        ///// Loads the page of items.
        ///// </summary>
        ///// <param name="pageIndex">Index of the page.</param>
        //protected virtual void LoadPage(int pageIndex)
        //{
        //    PopulatePage(pageIndex, FetchPage(pageIndex));
        //}


        ///// <summary>
        ///// Fetches the requested page from the IItemsProvider.
        ///// </summary>
        ///// <param name="pageIndex">Index of the page.</param>
        ///// <returns></returns>
        //protected IList<T> FetchPage(int pageIndex)
        //{
        //    return ItemsProvider.FetchRange(pageIndex * PageSize, PageSize);
        //}

        ///// <summary>
        ///// Fetches the count of itmes from the IItemsProvider.
        ///// </summary>
        ///// <returns></returns>
        //protected int FetchCount()
        //{
        //    return ItemsProvider.FetchCount();
        //}

        #endregion

        #region IList<T>, IList

        public int Count => _controller.Count();


        /// <summary>
        /// Gets the item at the specified index. This property will fetch
        /// the corresponding page from the IItemsProvider if required.
        /// </summary>
        /// <value></value>
        public T this[int index]
        {
            get
            {
                return _controller.Get(index);
            }
            set { throw new NotSupportedException(); }
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException(); }
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <remarks>
        /// This method should be avoided on large collections due to poor performance.
        /// </remarks>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }



        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// Always false.
        /// </returns>
        public bool Contains(T item)
        {
            return false;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public void Clear()
        {
            throw new NotSupportedException();
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        /// <returns>
        /// Always -1.
        /// </returns>
        public int IndexOf(T item)
        {
            return -1;
        }


        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }


        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        public object SyncRoot
        {
            get { return this; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }
        #endregion

        public void Add(T item)
        {
            //Do add, and add to end
        }

        public void Insert(int index, T item)
        {
            //Do add, and add to end
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }


        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public void Reset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

        public void OnInsert(T item, int index)
        {
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index);
            OnCollectionChanged(args);
        }

        public void OnRemove(T item, int index)
        {
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
            OnCollectionChanged(args);
        }

        public void OnSet(T oldValue, T newValue, int index)
        {
            var newItems = new List<T> {newValue};
            var oldItems = new List<T> {oldValue};
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, index);
            OnCollectionChanged(args);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }


    }
}
