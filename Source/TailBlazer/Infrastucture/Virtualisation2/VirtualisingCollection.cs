using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using DynamicData.Binding;

namespace TailBlazer.Infrastucture.Virtualisation2
{
    public class VirtualisingCollection<T> : AbstractNotifyPropertyChanged, IList<T>, IList, INotifyCollectionChanged
    {
        private readonly IVirtualController<T> _controller;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public VirtualisingCollection(IVirtualController<T> controller)
        {
            _controller = controller;
            var adder = controller.ItemsAdded.Subscribe(items =>
            {
                if (items.Length > 25)
                {
                    Reset();
                }
                else
                {
                    items.ForEach(t =>
                    {
                        OnInsert(t.Item, t.Index);
                    });
                }
            });
        }

        #region IList<T>, IList

        public int Count => _controller.Count();

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

       public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }
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

        public bool Contains(T item)
        {
            return false;
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        public int IndexOf(T item)
        {
            return _controller.IndexOf(item);
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
