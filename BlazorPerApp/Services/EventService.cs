using System.Collections.Generic;
using BlazorPerApp.Models;

namespace BlazorPerApp.Services
{
    public class EventService
    {
        private readonly List<EventItem> _events = new();
        public event System.Action? OnChange;
        private readonly Microsoft.Extensions.Logging.ILogger<EventService> _logger;
        private readonly LogStorage _logStorage;

        public EventService(Microsoft.Extensions.Logging.ILogger<EventService> logger, LogStorage logStorage)
        {
            _logger = logger;
            _logStorage = logStorage;
            _logger.LogInformation("EventService initialized");
        }

        public IReadOnlyList<EventItem> GetAll() => _events.AsReadOnly();

        public void Add(EventItem item)
        {
            if (item == null)
            {
                _logger.LogWarning("Attempted to add null EventItem");
                return;
            }

            // ensure attendees list isn't null
            var attendees = item.Attendees ?? new System.Collections.Generic.List<string>();

            // store a copy to avoid accidental external mutations
            var copy = new EventItem
            {
                Title = item.Title ?? string.Empty,
                Date = item.Date,
                Description = item.Description ?? string.Empty,
                IsAttending = item.IsAttending,
                Attendees = new System.Collections.Generic.List<string>(attendees)
            };

            _events.Add(copy);
            _logger.LogInformation("Event added: {Title} on {Date}", copy.Title, copy.Date);
            _ = _logStorage.AppendAsync(new Models.LogEntry
            {
                Timestamp = System.DateTime.UtcNow,
                Level = "Information",
                Category = nameof(EventService),
                Message = $"Event added: {copy.Title} on {copy.Date}",
            });
            NotifyStateChanged();
        }

        public bool AddAttendee(System.Guid eventId, string attendee)
        {
            var ev = _events.FirstOrDefault(x => x.Id == eventId);
            if (ev == null) return false;
            if (string.IsNullOrWhiteSpace(attendee)) return false;
            ev.Attendees.Add(attendee.Trim());
            _logger.LogInformation("Added attendee '{Attendee}' to event {EventId}", attendee, eventId);
            _ = _logStorage.AppendAsync(new Models.LogEntry
            {
                Timestamp = System.DateTime.UtcNow,
                Level = "Information",
                Category = nameof(EventService),
                Message = $"Added attendee '{attendee}' to event {eventId}",
            });
            NotifyStateChanged();
            return true;
        }

        public bool RemoveAttendee(System.Guid eventId, string attendee)
        {
            var ev = _events.FirstOrDefault(x => x.Id == eventId);
            if (ev == null) return false;
            var removed = ev.Attendees.Remove(attendee);
            if (removed)
            {
                NotifyStateChanged();
                _logger.LogInformation("Removed attendee '{Attendee}' from event {EventId}", attendee, eventId);
                _ = _logStorage.AppendAsync(new Models.LogEntry
                {
                    Timestamp = System.DateTime.UtcNow,
                    Level = "Information",
                    Category = nameof(EventService),
                    Message = $"Removed attendee '{attendee}' from event {eventId}",
                });
            }
            else
            {
                _logger.LogWarning("Failed to remove attendee '{Attendee}' from event {EventId}", attendee, eventId);
                _ = _logStorage.AppendAsync(new Models.LogEntry
                {
                    Timestamp = System.DateTime.UtcNow,
                    Level = "Warning",
                    Category = nameof(EventService),
                    Message = $"Failed to remove attendee '{attendee}' from event {eventId}",
                });
            }
            return removed;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public void Clear() => _events.Clear();
    }
}
