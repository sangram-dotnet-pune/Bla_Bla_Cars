namespace TripService.Models
{
    public class TripResponseDto
    {
        public Guid TripId { get; set; }
        public Guid DriverId { get; set; }

        public string DriverName { get; set; }
        public string? DriverMiniBio { get; set; }
        public string? DriverTravelPreferences { get; set; }
        public DriverVehicleDto? DriverVehicle { get; set; }

        public string StartLocation { get; set; }
        public string EndLocation { get; set; }

        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }

        public decimal PricePerSeat { get; set; }
        public TripStatus Status { get; set; }


        public List<string> MiddleCities { get; set; } = new();
        public string? Comment { get; set; }
    }

    public class DriverVehicleDto
    {
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public string? Color { get; set; }
    }
}
