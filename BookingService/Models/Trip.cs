namespace BookingService.Models
{
    public class Trip
    {
        public Guid TripId { get; set; }
        //  public Guid DriverId { get; set; } // comes from User Service

        public string StartLocation { get; set; }
        public string EndLocation { get; set; }

        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }

        public decimal PricePerSeat { get; set; }
        public TripStatus Status { get; set; }
    }
}
