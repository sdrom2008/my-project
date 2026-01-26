using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.DTOs
{
    public class SynerixisResponse
    {
        public string ConversationId { get; set; } = string.Empty;
        public List<ChatMessageDto> Messages { get; set; } = new();
        public bool Success { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }
}
