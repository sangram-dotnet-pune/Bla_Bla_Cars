namespace TripService.Models
{
    public class CreateBookingRequest
    {
        public Guid TripId { get; set; }

        public Guid PassengerId {  get; set; }
        public string PassengerName { get; set; }
        public int SeatsBooked { get; set; }
    }
}
