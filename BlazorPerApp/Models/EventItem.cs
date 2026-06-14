using System.ComponentModel.DataAnnotations;

namespace BlazorPerApp.Models
{
    public class EventItem
    {
        public EventItem()
        {
            Date = System.DateTime.Today;
            Id = System.Guid.NewGuid();
            Attendees = new System.Collections.Generic.List<string>();
        }

        public System.Guid Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Title is too long (100 character limit).")]
        public string Title { get; set; } = string.Empty;

        [Required]
        public System.DateTime Date { get; set; }

        [StringLength(1000, ErrorMessage = "Description is too long (1000 character limit).")]
        public string Description { get; set; } = string.Empty;

        public bool IsAttending { get; set; }

        public System.Collections.Generic.List<string> Attendees { get; set; }
    }
}
