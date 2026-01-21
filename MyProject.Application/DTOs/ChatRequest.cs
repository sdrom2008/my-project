using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Application.DTOs
{
    public class ChatRequest
    {
        public string ConversationId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
