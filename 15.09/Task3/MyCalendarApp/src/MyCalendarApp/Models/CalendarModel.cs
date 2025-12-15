using System;
using System.Collections.Generic;

namespace MyCalendarApp.Models
{
    public class CalendarModel
    {
        private List<EventItem> events;

        public List<EventItem> Events
        {
            get => events;
            set => events = value ?? new List<EventItem>();
        }

        public CalendarModel()
        {
            events = new List<EventItem>();
        }

        public void AddEvent(EventItem eventItem)
        {
            events.Add(eventItem);
        }

        public void RemoveEvent(EventItem eventItem)
        {
            events.Remove(eventItem);
        }

        public List<EventItem> GetEvents()
        {
            return events;
        }

        public List<EventItem> GetEventsByDate(DateTime date)
        {
            return events.FindAll(e => e.Date.Date == date.Date);
        }
    }
}