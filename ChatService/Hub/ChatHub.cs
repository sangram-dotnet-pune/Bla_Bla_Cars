using ChatService.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;


namespace ChatService.Hub
{
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly ChatDbContext _context;

        public ChatHub(ChatDbContext context)
        {
            _context = context;
        }

        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        }

        public async Task SendMessage(string conversationId, string senderId, string receiverId, string messageText)
        {
            var message = new Message
            {
                MessageId = Guid.NewGuid(),
                ConversationId = Guid.Parse(conversationId),
                SenderId = Guid.Parse(senderId),
                ReceiverId = Guid.Parse(receiverId),
                MessageText = messageText,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await Clients.Group(conversationId)
                .SendAsync("ReceiveMessage", senderId, messageText);
        }
    }
}
