using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripService.Models;

namespace TripService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripController(TripDbContext _context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Trip>>> getAllTrips()
        {
            return await _context.Trips.AsNoTracking().ToListAsync();
        }

        [HttpGet("{tripId:guid}")]
        public async Task<ActionResult<TripDto>> GetTripById(Guid tripId)
        {
            var trip = await _context.Trips.FirstOrDefaultAsync(x => x.TripId == tripId);

            if (trip == null)
                return NotFound();

            return Ok(new TripDto
            {
                TripId = tripId,
                AvailableSeats = trip.AvailableSeats,
                PricePerSeat = trip.PricePerSeat
            });
        }




        [HttpPost]
        public async Task<ActionResult<Trip>> CreateTrip([FromBody] Trip trip)
        {
            if (trip == null || !ModelState.IsValid)
            {
                return BadRequest("Invaild Data is given");
            }

            var tripRes= _context.Trips.Add(trip).Entity;
             await _context.SaveChangesAsync();
            return Ok(tripRes);


        }
        [HttpPut("update-seats/{tripId:guid}/{seats:int}")]
        public async Task<ActionResult> UpdateAvailableSeats(Guid tripId, int seats)
        {
            var trip = await _context.Trips.FirstOrDefaultAsync(x => x.TripId == tripId);

            if (trip == null)
                return NotFound();

            if (trip.AvailableSeats < seats)
                return BadRequest("Not enough seats available!");

            trip.AvailableSeats -= seats;

            await _context.SaveChangesAsync();
            return Ok(trip);
        }


        [HttpPut("restore-seats/{tripId:guid}/{seats:int}")]


        public async Task<ActionResult<Trip>> RestoreSeats([FromRoute] Guid tripId, int seats)
        {
            var trip = await _context.Trips.FirstOrDefaultAsync(x => x.TripId == tripId);

            if (trip == null)
                return NotFound();

            trip.AvailableSeats += seats;

            await _context.SaveChangesAsync();
            return Ok(trip);
        }



    }
}
