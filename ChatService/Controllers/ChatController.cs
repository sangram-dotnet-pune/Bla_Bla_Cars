using ChatService.Dtos;
using ChatService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatDbContext _context;

        public ChatController(ChatDbContext context)
        {
            _context = context;
        }

        [HttpPost("conversation")]
        public async Task<IActionResult> CreateConversation(CreateConversationDto dto)
        {
            var existing = await _context.Conversations
                .FirstOrDefaultAsync(c => c.BookingId == dto.BookingId);

            if (existing != null)
                return Ok(existing);

            var conversation = new Conversation
            {
                ConversationId = Guid.NewGuid(),
                BookingId = dto.BookingId,
                PassengerId = dto.PassengerId,
                DriverId = dto.DriverId
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return Ok(conversation);
        }


        [HttpPost("message")]
        public async Task<IActionResult> SendMessage(SendMessageDto dto)
        {
            var message = new Message
            {
                MessageId = Guid.NewGuid(),
                ConversationId = dto.ConversationId,
                SenderId = dto.SenderId,
                ReceiverId = dto.ReceiverId,
                MessageText = dto.MessageText
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(message);
        }


        [HttpGet("messages/{conversationId}")]
        public async Task<IActionResult> GetMessages(Guid conversationId)
        {
            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return Ok(messages);
        }
    }

}

