using System;
using System.Collections.Generic;

namespace Platform.Dal.Decorators.Abstract
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class EventDataBase
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData1"></param>
        /// <param name="eventData2"></param>
        /// <returns></returns>
        public static EventDataBase operator +(EventDataBase eventData1, EventDataBase eventData2)  // explicit byte to digit conversion operator
        {
            var events = new List<EventData>();
            AddEventData(eventData1, events);
            AddEventData(eventData2, events);
            return new EventDatas(events);
        }


        private static void AddEventData(EventDataBase eventData, List<EventData> events)
        {
            if (eventData is EventData)
                events.Add(eventData as EventData);
            else if (eventData is EventDatas)
                events.AddRange((eventData as EventDatas));
            else
                throw new InvalidOperationException("Пользовательские сданные событий  должны быть унаследованы от EventData ");
        }

        public EventDatas ToEventDatas()
        {
            if (this is EventDatas)
                return this as EventDatas;
            else if (this is EventData)
                return new EventDatas(new List<EventData>() { this as EventData });
            else
                throw new InvalidOperationException("Пользовательские сданные событий  должны быть унаследованы от EventData ");
        }

    }
}
