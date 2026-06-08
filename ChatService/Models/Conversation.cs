using System.ComponentModel.DataAnnotations;

namespace ChatService.Models
{
    public class Conversation
    {
        [Key]
        public Guid ConversationId { get; set; }

        // Link to Booking Service
        public Guid BookingId { get; set; }

        public Guid PassengerId { get; set; }
        public Guid DriverId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Message> Messages { get; set; }

    }
}
