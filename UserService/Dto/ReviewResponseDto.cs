namespace UserService.Dto;

public class ReviewResponseDto
{
    public Guid Id { get; set; }
    public Guid RideId { get; set; }
    public Guid BookingId { get; set; }
    public Guid ReviewerId { get; set; }
    public Guid RevieweeId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UserReviewSummaryDto
{
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<ReceivedReviewDto> Reviews { get; set; } = new();
}

public class ReceivedReviewDto
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public ReviewerDto Reviewer { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class ReviewerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}

public class ReviewRideStatusDto
{
    public Guid RideId { get; set; }
    public List<ReviewPassengerStatusDto> Passengers { get; set; } = new();
}

public class ReviewPassengerStatusDto
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool HasReviewed { get; set; }
    public int? Rating { get; set; }
}
