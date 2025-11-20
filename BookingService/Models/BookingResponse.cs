namespace BookingService.Models
{
    public class BookingResponse
    {
        public Guid BookingId { get; set; }
        public Guid TripId { get; set; }
        public int SeatsBooked { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }
}
