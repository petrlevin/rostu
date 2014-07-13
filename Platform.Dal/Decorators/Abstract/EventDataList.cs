using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Dal.Decorators.Abstract
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class EventDataList<T>:EventData, IList<T>
    {

        private readonly List<T> _innerList;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public EventDataList(IEnumerable<T> list)
        {
            if (list == null) throw new ArgumentNullException("list");
            _innerList = list.ToList();
        }

        public EventDataList()
        {
            _innerList = new List<T>();
        }



        public IEnumerator<T> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _innerList.Add(item);
        }

        public void Clear()
        {
            _innerList.Clear();
        }

        public bool Contains(T item)
        {
            return _innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _innerList.CopyTo(array,arrayIndex);
        }

        public bool Remove(T item)
        {
            return _innerList.Remove(item);
        }

        public int Count
        {
            get { return _innerList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(T item)
        {
            return _innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _innerList.Insert(index,item);
        }

        public void RemoveAt(int index)
        {
            _innerList.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return _innerList[index]; }
            set { _innerList[index] = value; }
        }
    }
}
