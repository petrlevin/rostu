using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Dal.Decorators.Abstract
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class EventDatas : EventDataBase, IList<EventData>
    {

        private readonly List<EventData> _eventDatas = new List<EventData>();

        #region IList implementation

        public IEnumerator<EventData> GetEnumerator()
        {
            return _eventDatas.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _eventDatas.GetEnumerator();
        }


        public void Add(EventData item)
        {
            _eventDatas.Add(item);
        }

        public void Clear()
        {
            _eventDatas.Clear();
        }

        public bool Contains(EventData item)
        {
            return _eventDatas.Contains(item);
        }

        public void CopyTo(EventData[] array, int arrayIndex)
        {
            _eventDatas.CopyTo(array,arrayIndex);
        }

        public bool Remove(EventData item)
        {
            return _eventDatas.Remove(item);
        }

        public int Count
        {
            get { return _eventDatas.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(EventData item)
        {
            return _eventDatas.IndexOf(item);
        }

        public void Insert(int index, EventData item)
        {
            _eventDatas.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _eventDatas.RemoveAt(index);
        }

        public EventData this[int index]
        {
            get { return _eventDatas[index]; }
            set { _eventDatas[index] = value; }
        }

        #endregion

        internal EventDatas(IEnumerable<EventData> eventData)
        {
            _eventDatas = eventData.ToList();
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <typeparam name="TEventData"></typeparam>
        /// <returns></returns>
        public EventDatas ForEach<TEventData>(Action<TEventData> action) where TEventData : EventData
        {
            _eventDatas.OfType<TEventData>().ToList().ForEach(action);
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <typeparam name="TEventData"></typeparam>
        /// <returns></returns>
        public EventDatas ForSingle<TEventData>(Action<TEventData> action) where TEventData : EventData
        {
            var ed =_eventDatas.OfType<TEventData>().SingleOrDefault();
            if (ed != null)
                action(ed);
            return this;
        }

        


        public EventDatas Remove<TEventData>(Func<TEventData, bool> condition) where TEventData : EventData
        {
            _eventDatas.RemoveAll(ed => (ed is TEventData) && condition((TEventData)ed));
            return this;
        }




    }
}
