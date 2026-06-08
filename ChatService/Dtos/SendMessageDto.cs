namespace ChatService.Dtos
{
    public class SendMessageDto
    {
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string MessageText { get; set; }
    }
}
