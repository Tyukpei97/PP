using System;
using System.Collections.Generic;
using System.Linq;
using MyCalendarApp.Models;
using MyCalendarApp.Services;

namespace MyCalendarApp.Controllers
{
    public class CalendarController
    {
        private readonly CalendarModel _calendarModel;
        private readonly EventService _eventService;

        public CalendarController(CalendarModel calendarModel, EventService eventService)
        {
            _calendarModel = calendarModel;
            _eventService = eventService;
        }

        public void AddEvent(EventItem eventItem)
        {
            _eventService.AddEvent(eventItem);
            UpdateCalendar();
        }

        public void EditEvent(EventItem eventItem)
        {
            _eventService.UpdateEvent(eventItem);
            UpdateCalendar();
        }

        public void DeleteEvent(Guid eventId)
        {
            _eventService.DeleteEvent(eventId);
            UpdateCalendar();
        }

        public List<EventItem> GetEvents(DateTime date)
        {
            return _calendarModel.Events.Where(e => e.Date.Date == date.Date).ToList();
        }

        private void UpdateCalendar()
        {
            _calendarModel.Events = _eventService.GetAllEvents();
        }
    }
}