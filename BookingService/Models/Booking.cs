namespace BookingService.Models
{
    public class Booking
    {
        public Guid BookingId { get; set; }

        public Guid TripId { get; set; } 
        public Guid PassengerId { get; set; } 


     public string PassengerName {  get; set; }

        public Guid DriverId { get; set; }

        public int SeatsBooked { get; set; }
        public decimal TotalAmount { get; set; }

        public BookingStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

      
        //public Guid? PaymentId { get; set; }
    }
}
