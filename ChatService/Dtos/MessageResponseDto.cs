namespace ChatService.Dtos
{
    public class MessageResponseDto
    {
        public Guid MessageId { get; set; }
        public Guid SenderId { get; set; }
        public string MessageText { get; set; }
        public DateTime SentAt { get; set; }
    }
}
