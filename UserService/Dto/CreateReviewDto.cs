using System.ComponentModel.DataAnnotations;

namespace UserService.Dto;

public class CreateReviewDto
{
    [Required]
    public Guid RideId { get; set; }

    [Required]
    public Guid BookingId { get; set; }

    [Required]
    public Guid RevieweeId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
}
