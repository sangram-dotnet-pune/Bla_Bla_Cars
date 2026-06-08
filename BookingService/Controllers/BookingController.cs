
using BookingService.Models;
using BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TripService.Models;

namespace BookingService.Controllers
{



    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly BookingDbContext _context;
        private readonly TripClientService _tripClient;
        private readonly UserClientService _userClientService;

        public BookingController(BookingDbContext context, TripClientService tripClient, UserClientService userClientService)
        {
            _context = context;
            _tripClient = tripClient;
            _userClientService = userClientService;
        }

        [HttpGet]
        [Authorize]

        public async Task<ActionResult<List<BookingResponse>>> GetAllBookings()
        {
            var userIdStr =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value ??
                User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("User id claim missing or invalid.");

            var token = Request.Headers["Authorization"]
                            .ToString()
                            .Replace("Bearer ", "");

            var user = await _userClientService.GetCurrentUserAsync(userId, token);
            if (user == null)
                return NotFound("User not found.");

            var bookings = await _context.Bookings
                .AsNoTracking()
                .Where(b => b.Status != BookingStatus.Cancelled
                         && b.PassengerId == userId)
                .ToListAsync();

            var response = new List<BookingResponse>();

            foreach (var booking in bookings)
            {
                var trip = await _tripClient.GetTripByIdAsync(booking.TripId);

                response.Add(new BookingResponse
                {
                    BookingId = booking.BookingId,
                    TripId = booking.TripId,
                    DriverName = trip?.DriverName ?? "Unknown",
                    PassengerName = booking.PassengerName,
                    SeatsBooked = booking.SeatsBooked,
                    Status = booking.Status.ToString(),
                    TotalAmount = booking.TotalAmount
                });
            }

            return Ok(response);
        }



   
        [Authorize]
        [HttpGet("trip/{tripId}")]
        public async Task<ActionResult<List<BookingResponse>>>getAllBookingForTrip(Guid tripId)
        {

            var userIdStr =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value ??
                User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("User id claim missing or invalid.");

            var token = Request.Headers["Authorization"]
                            .ToString()
                            .Replace("Bearer ", "");

            var user = await _userClientService.GetCurrentUserAsync(userId, token);
            if (user == null)
                return NotFound("User not found.");

            var bookings = await _context.Bookings
               .AsNoTracking()
               .Where(b =>
                         b.TripId==tripId
                        && b.DriverId == userId)
               .ToListAsync();

            return Ok(bookings);
        }
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateBooking([FromBody] CreateBookingRequest booking)
        {
            var trip = await _tripClient.GetTripByIdAsync(booking.TripId);
            if (trip == null)
                return NotFound("Trip does not exist!");

       
            if (trip.AvailableSeats < booking.SeatsBooked)
                return BadRequest("Not enough seats available!");

           
            var seatUpdated = await _tripClient.UpdateSeatsAsync(booking.TripId, booking.SeatsBooked);
            if (!seatUpdated)
                return StatusCode(500, "Failed to update seats");


            Booking newBooking = new Booking()
            {
                TripId = booking.TripId,
                PassengerId = booking.PassengerId,
                SeatsBooked = booking.SeatsBooked,
                PassengerName = booking.PassengerName,
                DriverId=trip.DriverId,
                CreatedAt = DateTime.UtcNow,
                CancelledAt = null,
                TotalAmount = booking.SeatsBooked * trip.PricePerSeat,
                Status = BookingStatus.Pending
            };
           
          

            await _context.Bookings.AddAsync(newBooking);
            await _context.SaveChangesAsync();

            return Ok(booking);
        }

        //passanger feature to cancel booking 

        [HttpPut("cancel/{bookingId:guid}")]
        [Authorize]
        public async Task<ActionResult> CancelBooking(Guid bookingId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null) return NotFound();

            if (booking.Status == BookingStatus.Cancelled)
                return BadRequest("Already cancelled");

            // Restore seats
            var seatRestored = await _tripClient.RestoreSeatsAsync(booking.TripId, booking.SeatsBooked);
            if (!seatRestored)
                return StatusCode(500, "Seat restoration failed");

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok("Booking cancelled successfully");
        }
      
        [HttpPut("{bookingId:guid}/approve")]
        [Authorize]
        public async Task<ActionResult> ApproveBooking(Guid bookingId)
        {
            // extract user id from claims
            var userIdStr =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value ??
                User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("User id claim missing or invalid.");

            // extract token and validate user exists
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = await _userClientService.GetCurrentUserAsync(userId, token);
            if (user == null)
                return NotFound("User not found.");

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null)
                return NotFound("Booking not found.");

            // only driver can approve
            if (booking.DriverId != userId)
                return Forbid("Only the driver can approve this booking.");

            if (booking.Status == BookingStatus.Confirmed)
                return BadRequest("Booking is already approved/confirmed.");

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Failed)
                return BadRequest("Cannot approve a cancelled or failed booking.");

            booking.Status = BookingStatus.Confirmed;
            await _context.SaveChangesAsync();

            return Ok(new { bookingId = booking.BookingId, status = booking.Status.ToString(), message = "Booking approved." });
        }

        [HttpPut("{bookingId:guid}/reject")]
        [Authorize]
        public async Task<ActionResult> RejectBooking(Guid bookingId)
        {
            // extract user id from claims
            var userIdStr =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value ??
                User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("User id claim missing or invalid.");

            // extract token and validate user exists
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = await _userClientService.GetCurrentUserAsync(userId, token);
            if (user == null)
                return NotFound("User not found.");

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null)
                return NotFound("Booking not found.");

            // only driver can reject
            if (booking.DriverId != userId)
                return Forbid("Only the driver can reject this booking.");

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Failed)
                return BadRequest("Booking is already cancelled or rejected.");

            // restore seats back to trip
            var seatRestored = await _tripClient.RestoreSeatsAsync(booking.TripId, booking.SeatsBooked);
            if (!seatRestored)
                return StatusCode(500, "Failed to restore seats on trip.");

            booking.Status = BookingStatus.Rejected;
            booking.CancelledAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { bookingId = booking.BookingId, status = booking.Status.ToString(), message = "Booking rejected and seats restored." });
        }






    }
}
