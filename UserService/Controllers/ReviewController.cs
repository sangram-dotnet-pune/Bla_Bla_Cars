using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Dto;
using UserService.Services;
using System.Security.Claims;

namespace UserService.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewController(ReviewService reviewService) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto request, CancellationToken cancellationToken)
    {
        var reviewerId = GetUserId();
        if (reviewerId is null)
            return Unauthorized("User ID claim missing or invalid.");

        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(token))
            return Unauthorized("Bearer token is required.");

        var result = await reviewService.CreateAsync(reviewerId.Value, request, token, cancellationToken);
        if (result.Review is null)
            return StatusCode(result.Status, new { message = result.Error });

        return StatusCode(StatusCodes.Status201Created, ToResponse(result.Review));
    }

    [HttpGet("/api/users/{userId:guid}/reviews")]
    public async Task<ActionResult<UserReviewSummaryDto>> GetReceived(Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await reviewService.GetReceivedAsync(userId, cancellationToken));
    }

    [Authorize]
    [HttpGet("ride/{rideId:guid}")]
    public async Task<IActionResult> GetRideStatus(Guid rideId, CancellationToken cancellationToken)
    {
        var requesterId = GetUserId();
        if (requesterId is null)
            return Unauthorized("User ID claim missing or invalid.");
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(token))
            return Unauthorized("Bearer token is required.");

        var result = await reviewService.GetRideStatusAsync(rideId, requesterId.Value, token, cancellationToken);
        return result.Value is not null
            ? Ok(result.Value)
            : StatusCode(result.Status, new { message = result.Error });
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirst("userId")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    private static ReviewResponseDto ToResponse(Models.Review review) => new()
    {
        Id = review.Id,
        RideId = review.RideId,
        BookingId = review.BookingId,
        ReviewerId = review.ReviewerId,
        RevieweeId = review.RevieweeId,
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAt = review.CreatedAt,
        UpdatedAt = review.UpdatedAt
    };
}
