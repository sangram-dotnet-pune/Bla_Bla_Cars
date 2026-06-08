namespace ChatService.Dtos
{
    public class CreateConversationDto
    {
        public Guid BookingId { get; set; }
        public Guid PassengerId { get; set; }
        public Guid DriverId { get; set; }
    }
}
