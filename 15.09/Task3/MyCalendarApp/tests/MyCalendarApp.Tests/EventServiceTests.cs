using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyCalendarApp.Services;
using MyCalendarApp.Models;

namespace MyCalendarApp.Tests
{
    [TestClass]
    public class EventServiceTests
    {
        private EventService _eventService;

        [TestInitialize]
        public void Setup()
        {
            _eventService = new EventService();
        }

        [TestMethod]
        public void AddEvent_ShouldAddEvent()
        {
            var eventItem = new EventItem
            {
                Title = "Test Event",
                Date = DateTime.Now,
                Time = TimeSpan.FromHours(10),
                Description = "This is a test event."
            };

            _eventService.AddEvent(eventItem);
            var events = _eventService.GetAllEvents();

            Assert.IsTrue(events.Contains(eventItem));
        }

        [TestMethod]
        public void RemoveEvent_ShouldRemoveEvent()
        {
            var eventItem = new EventItem
            {
                Title = "Test Event",
                Date = DateTime.Now,
                Time = TimeSpan.FromHours(10),
                Description = "This is a test event."
            };

            _eventService.AddEvent(eventItem);
            _eventService.RemoveEvent(eventItem);
            var events = _eventService.GetAllEvents();

            Assert.IsFalse(events.Contains(eventItem));
        }

        [TestMethod]
        public void UpdateEvent_ShouldUpdateEvent()
        {
            var eventItem = new EventItem
            {
                Title = "Test Event",
                Date = DateTime.Now,
                Time = TimeSpan.FromHours(10),
                Description = "This is a test event."
            };

            _eventService.AddEvent(eventItem);

            eventItem.Title = "Updated Event";
            _eventService.UpdateEvent(eventItem);
            var updatedEvent = _eventService.GetEventById(eventItem.Id);

            Assert.AreEqual("Updated Event", updatedEvent.Title);
        }

        [TestMethod]
        public void GetAllEvents_ShouldReturnAllEvents()
        {
            var event1 = new EventItem
            {
                Title = "Event 1",
                Date = DateTime.Now,
                Time = TimeSpan.FromHours(10),
                Description = "First event."
            };

            var event2 = new EventItem
            {
                Title = "Event 2",
                Date = DateTime.Now,
                Time = TimeSpan.FromHours(11),
                Description = "Second event."
            };

            _eventService.AddEvent(event1);
            _eventService.AddEvent(event2);
            var events = _eventService.GetAllEvents();

            Assert.AreEqual(2, events.Count);
        }
    }
}