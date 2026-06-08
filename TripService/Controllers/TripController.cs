using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using TripService.Models;
using TripService.Service;

namespace TripService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TripController : ControllerBase
    {

        private readonly UserClientService _userClientService;
        private readonly TripDbContext _context;

        public TripController(UserClientService userClientService, TripDbContext context)
        {
            _userClientService = userClientService;
            _context = context;
        }

        [HttpGet("my-trips")]
        public async Task<ActionResult<List<TripResponseDto>>> getAllTripsUser()
        {

          
          
            var userIdStr =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value ??
                User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("User id claim missing or invalid.");

            var tokan = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = await _userClientService.GetCurrentUserAsync(userId,tokan);
            if (user == null)
                return NotFound("User not found.");

            // 🔹 Get trips
            var trips = await _context.Trips
                .AsNoTracking()
                .Where(t => t.DriverId == userId)
                .ToListAsync();

            // 🔹 Map to DTO
            var tripDtos = trips.Select(t => new TripResponseDto
            {
                TripId = t.TripId,
                DriverId = t.DriverId,
                DriverName = user.FullName,  // 👈 from client service

                StartLocation = t.StartLocation,
                EndLocation = t.EndLocation,
                DepartureTime = t.DepartureTime,
                ArrivalTime = t.ArrivalTime,
                TotalSeats = t.TotalSeats,
                AvailableSeats = t.AvailableSeats,
                PricePerSeat = t.PricePerSeat,
                Status = t.Status,
                MiddleCities = t.MiddleCities,
                Comment = t.Comment
            }).ToList();

            return Ok(tripDtos);
        }

     
        [HttpGet]
        public async Task<ActionResult<List<TripResponseDto>>> getAllTrips()
        {
           
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

          
            var trips = await _context.Trips
                .AsNoTracking()
                .ToListAsync();

            if (trips == null || trips.Count == 0)
                return Ok(new List<TripResponseDto>());

            var driverIds = trips
                .Where(t => t.DriverId != Guid.Empty)
                .Select(t => t.DriverId)
                .Distinct()
                .ToList();

           
            var userTasks = driverIds.ToDictionary(
                id => id,
                id => _userClientService.GetCurrentUserAsync(id, token)
            );

            try
            {
                await Task.WhenAll(userTasks.Values);
            }
            catch
            {
               
            }

            var usersById = userTasks.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.IsCompletedSuccessfully ? kvp.Value.Result : null
            );

          
            var tripDtos = trips.Select(t =>
            {
                usersById.TryGetValue(t.DriverId, out var user);

                return new TripResponseDto
                {
                    TripId = t.TripId,
                    DriverId = t.DriverId,
                    DriverName = user?.FullName ?? string.Empty,

                    StartLocation = t.StartLocation,
                    EndLocation = t.EndLocation,
                    DepartureTime = t.DepartureTime,
                    ArrivalTime = t.ArrivalTime,
                    TotalSeats = t.TotalSeats,
                    AvailableSeats = t.AvailableSeats,
                    PricePerSeat = t.PricePerSeat,
                    Status = t.Status,
                    MiddleCities = t.MiddleCities,
                    Comment = t.Comment
                };
            }).ToList();

            return Ok(tripDtos);
        }

        [HttpGet("{tripId:guid}")]
        public async Task<ActionResult<TripResponseDto>> GetTripById(Guid tripId)
        {
            var trip = await _context.Trips
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TripId == tripId);

            if (trip == null)
                return NotFound();

            var tokan = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = await _userClientService.GetCurrentUserAsync(trip.DriverId,tokan);

            if (user == null)
                return NotFound("Driver not found.");

          
            var tripDto = new TripResponseDto
            {
                TripId = trip.TripId,
                DriverId = trip.DriverId,
                DriverName = user.FullName, 

                StartLocation = trip.StartLocation,
                EndLocation = trip.EndLocation,
                DepartureTime = trip.DepartureTime,
                ArrivalTime = trip.ArrivalTime,
                TotalSeats = trip.TotalSeats,
                AvailableSeats = trip.AvailableSeats,
                PricePerSeat = trip.PricePerSeat,
                Status = trip.Status,
                MiddleCities = trip.MiddleCities,
                Comment = trip.Comment
            };

            return Ok(tripDto);
        }



        [HttpPost]
        public async Task<ActionResult<Trip>> CreateTrip([FromBody] Trip trip)
        {
            if (trip == null || !ModelState.IsValid)
                return BadRequest("Invalid Data is given");

            // 🔥 Get userId from JWT
            var userIdClaim = User.FindFirst("userId");

            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            trip.DriverId = Guid.Parse(userIdClaim.Value);

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            return Ok(trip);
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
