using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Application.DTOs
{
    public class ChatMessageReplyDto
    {
        public Guid ConversationId { get; set; }
        public List<ChatMessageItemDto> Messages { get; set; } = new();
    }
}
