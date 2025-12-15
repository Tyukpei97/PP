using System;
using System.Collections.Generic;
using MyCalendarApp.Models;

namespace MyCalendarApp.Services
{
    public class EventService
    {
        private List<EventItem> events;

        public EventService()
        {
            events = new List<EventItem>();
        }

        public void AddEvent(EventItem eventItem)
        {
            events.Add(eventItem);
        }

        public void UpdateEvent(EventItem eventItem)
        {
            var existingEvent = events.Find(e => e.Id == eventItem.Id);
            if (existingEvent != null)
            {
                existingEvent.Title = eventItem.Title;
                existingEvent.Date = eventItem.Date;
                existingEvent.Time = eventItem.Time;
                existingEvent.Description = eventItem.Description;
            }
        }

        public void RemoveEvent(EventItem eventItem)
        {
            if (eventItem == null) return;
            DeleteEvent(eventItem.Id);
        }

        public void DeleteEvent(Guid eventId)
        {
            var eventToRemove = events.Find(e => e.Id == eventId);
            if (eventToRemove != null)
            {
                events.Remove(eventToRemove);
            }
        }

        public List<EventItem> GetAllEvents()
        {
            return events;
        }

        public EventItem GetEventById(Guid eventId)
        {
            return events.Find(e => e.Id == eventId);
        }
    }
}