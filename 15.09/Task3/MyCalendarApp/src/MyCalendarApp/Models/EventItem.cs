using System;

namespace MyCalendarApp.Models
{
    public class EventItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Description { get; set; }

        public EventItem()
        {
            Id = Guid.NewGuid();
            Title = string.Empty;
            Date = DateTime.Now.Date;
            Time = TimeSpan.Zero;
            Description = string.Empty;
        }

        public EventItem(string title, DateTime date, TimeSpan time, string description)
        {
            Id = Guid.NewGuid();
            Title = title;
            Date = date;
            Time = time;
            Description = description;
        }

        public override string ToString()
        {
            return $"{Date:d} {Time:hh\\:mm} - {Title}";
        }
    }
}