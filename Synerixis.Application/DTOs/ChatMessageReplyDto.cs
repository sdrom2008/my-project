using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.DTOs
{
    public class ChatMessageReplyDto
    {
        public Guid ConversationId { get; set; }
        public List<ChatMessageDto> Messages { get; set; } = new();  // 统一用 ChatMessageDto
    }
}
