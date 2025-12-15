using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MyCalendarApp.Models;

namespace MyCalendarApp.Services
{
    public class PersistenceService
    {
        private readonly string _filePath;

        public PersistenceService(string filePath)
        {
            _filePath = filePath;
        }

        public async Task SaveEventsAsync(List<EventItem> events)
        {
            var json = JsonSerializer.Serialize(events);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<List<EventItem>> LoadEventsAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<EventItem>();
            }

            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<EventItem>>(json) ?? new List<EventItem>();
        }
    }
}