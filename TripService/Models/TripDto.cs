namespace TripService.Models
{
    public class TripDto
    {
        public Guid TripId { get; set; }
        public int AvailableSeats { get; set; }
        public decimal PricePerSeat { get; set; }
    }
}
