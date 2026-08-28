using Microsoft.EntityFrameworkCore;
using UserService.Dto;
using UserService.Models;

namespace UserService.Services;

public class ReviewService(
    AppUserDbContext context,
    TripReviewClient tripClient,
    BookingReviewClient bookingClient)
{
    public async Task<(Review? Review, string? Error, int Status)> CreateAsync(
        Guid reviewerId, CreateReviewDto request, string token, CancellationToken cancellationToken)
    {
        if (request.Rating is < 1 or > 5)
            return (null, "Rating must be between 1 and 5.", 400);
        if (request.RideId == Guid.Empty || request.BookingId == Guid.Empty || request.RevieweeId == Guid.Empty)
            return (null, "Ride, booking and reviewee are required.", 400);

        var reviewer = await context.AppUsers.FindAsync([reviewerId], cancellationToken);
        if (reviewer is null)
            return (null, "Authenticated user was not found.", 401);
        if (reviewerId == request.RevieweeId)
            return (null, "A user cannot review themselves.", 400);
        if (await context.AppUsers.FindAsync([request.RevieweeId], cancellationToken) is null)
            return (null, "Reviewee not found.", 404);

        (TripReviewValidation? Value, System.Net.HttpStatusCode Status) tripResult;
        try
        {
            tripResult = await tripClient.GetAsync(request.RideId, token, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            return (null, "Trip service validation failed.", 502);
        }
        if (tripResult.Value is null)
            return (null, tripResult.Status == System.Net.HttpStatusCode.NotFound ? "Ride not found." : "Trip service validation failed.", tripResult.Status == System.Net.HttpStatusCode.NotFound ? 404 : 502);
        var trip = tripResult.Value;
        if (!string.Equals(trip.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            return (null, "Reviews are available only after a ride is completed.", 400);

        (BookingReviewValidation? Value, System.Net.HttpStatusCode Status) bookingResult;
        try
        {
            bookingResult = await bookingClient.GetAsync(request.BookingId, token, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            return (null, "Booking service validation failed.", 502);
        }
        var booking = bookingResult.Value;
        if (booking is null)
            return (null, bookingResult.Status == System.Net.HttpStatusCode.NotFound ? "Booking not found." : "Booking service validation failed.", bookingResult.Status == System.Net.HttpStatusCode.NotFound ? 404 : 502);
        if (booking.TripId != request.RideId || booking.TripId != trip.TripId)
            return (null, "Booking does not belong to the specified ride.", 400);
        if (!string.Equals(booking.TripStatus, "Completed", StringComparison.OrdinalIgnoreCase))
            return (null, "Reviews are available only after a ride is completed.", 400);
        if (!string.Equals(booking.Status, "Confirmed", StringComparison.OrdinalIgnoreCase))
            return (null, "Only confirmed bookings can be reviewed.", 400);

        var reviewerIsPassenger = reviewerId == booking.PassengerId;
        var reviewerIsDriver = reviewerId == trip.DriverId && reviewerId == booking.DriverId;
        if (!reviewerIsPassenger && !reviewerIsDriver)
            return (null, "Reviewer did not participate in this ride.", 403);

        var expectedReviewee = reviewerIsPassenger ? trip.DriverId : booking.PassengerId;
        if (request.RevieweeId != expectedReviewee)
            return (null, "Invalid reviewer/reviewee relationship.", 400);
        if (await context.Reviews.AnyAsync(r =>
                r.BookingId == request.BookingId &&
                r.ReviewerId == reviewerId &&
                r.RevieweeId == request.RevieweeId, cancellationToken))
            return (null, "Review already exists.", 409);

        var now = DateTime.UtcNow;
        var review = new Review
        {
            RideId = request.RideId,
            BookingId = request.BookingId,
            ReviewerId = reviewerId,
            RevieweeId = request.RevieweeId,
            Rating = request.Rating,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };
        context.Reviews.Add(review);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return (null, "Review already exists or could not be saved.", 409);
        }
        return (review, null, 201);
    }

    public async Task<UserReviewSummaryDto> GetReceivedAsync(Guid userId, CancellationToken cancellationToken)
    {
        var query = context.Reviews.AsNoTracking()
            .Where(r => r.RevieweeId == userId)
            .Include(r => r.Reviewer);
        var totalReviews = await query.CountAsync(cancellationToken);
        var averageRating = totalReviews == 0
            ? 0
            : Math.Round(await query.AverageAsync(r => (double)r.Rating, cancellationToken), 2);
        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
        return new UserReviewSummaryDto
        {
            TotalReviews = totalReviews,
            AverageRating = averageRating,
            Reviews = reviews.Select(r => new ReceivedReviewDto
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                Reviewer = new ReviewerDto { Id = r.ReviewerId, Name = r.Reviewer.FullName }
            }).ToList()
        };
    }

    public async Task<(ReviewRideStatusDto? Value, string? Error, int Status)> GetRideStatusAsync(
        Guid rideId, Guid requesterId, string token, CancellationToken cancellationToken)
    {
        (ReviewParticipants? Value, System.Net.HttpStatusCode Status) result;
        try
        {
            result = await bookingClient.GetParticipantsAsync(rideId, token, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            return (null, "Booking service validation failed.", 502);
        }
        if (result.Value is null)
            return (null, result.Status == System.Net.HttpStatusCode.NotFound ? "Ride not found." : "Booking service validation failed.", result.Status == System.Net.HttpStatusCode.NotFound ? 404 : 502);
        if (!string.Equals(result.Value.TripStatus, "Completed", StringComparison.OrdinalIgnoreCase))
            return (null, "Review status is available only after a ride is completed.", 400);

        var isDriver = result.Value.DriverId == requesterId;
        var bookings = isDriver
            ? result.Value.Bookings
            : result.Value.Bookings.Where(b => b.PassengerId == requesterId).ToList();
        if (!isDriver && bookings.Count == 0)
            return (null, "User did not participate in this ride.", 403);

        var statuses = new List<ReviewPassengerStatusDto>();
        foreach (var booking in bookings)
        {
            var revieweeId = isDriver ? booking.PassengerId : result.Value.DriverId;
            var review = await context.Reviews.AsNoTracking().FirstOrDefaultAsync(r =>
                r.BookingId == booking.BookingId && r.ReviewerId == requesterId && r.RevieweeId == revieweeId, cancellationToken);
            var user = await context.AppUsers.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == revieweeId, cancellationToken);
            statuses.Add(new ReviewPassengerStatusDto
            {
                BookingId = booking.BookingId,
                UserId = revieweeId,
                Name = user?.FullName ?? string.Empty,
                HasReviewed = review is not null,
                Rating = review?.Rating
            });
        }
        return (new ReviewRideStatusDto { RideId = rideId, Passengers = statuses }, null, 200);
    }
}
