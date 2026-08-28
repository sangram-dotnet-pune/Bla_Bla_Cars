using System.ComponentModel.DataAnnotations;

namespace UserService.Models;

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // These are references to records owned by Trip and Booking services.
    public Guid RideId { get; set; }
    public Guid BookingId { get; set; }

    public Guid ReviewerId { get; set; }
    public Guid RevieweeId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public AppUser Reviewer { get; set; } = null!;
    public AppUser Reviewee { get; set; } = null!;
}
