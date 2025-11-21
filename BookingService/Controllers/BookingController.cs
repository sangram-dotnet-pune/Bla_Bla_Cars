
using BookingService.Services;
using BookingService.Models;
using Microsoft.AspNetCore.Mvc;
using TripService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Controllers
{



    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly BookingDbContext _context;
        private readonly TripClientService _tripClient;

        public BookingController(BookingDbContext context, TripClientService tripClient)
        {
            _context = context;
            _tripClient = tripClient;
        }

        [HttpGet]

        public async Task<ActionResult<List<BookingResponse>>> GetAllBookings()
        {
            return await _context.Bookings
                .AsNoTracking()
                .Select(b => new BookingResponse
                {
                    BookingId = b.BookingId,
                    TripId = b.TripId,
                  PassengerName = b.PassengerName,
                    SeatsBooked = b.SeatsBooked,
                    Status=b.Status.ToString(),
                     TotalAmount = b.TotalAmount
                })
                .ToListAsync();
        }
        [HttpPost]
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
                SeatsBooked = booking.SeatsBooked,
                PassengerName = booking.PassengerName,

                CreatedAt = DateTime.UtcNow,
                CancelledAt =null,
              TotalAmount = booking.SeatsBooked * trip.PricePerSeat,
                Status = BookingStatus.Confirmed
            };
           
          

            await _context.Bookings.AddAsync(newBooking);
            await _context.SaveChangesAsync();

            return Ok(booking);
        }

        [HttpPut("cancel/{bookingId:guid}")]
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




    }
}
