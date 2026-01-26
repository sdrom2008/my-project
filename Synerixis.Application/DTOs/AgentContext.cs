using Synerixis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.DTOs
{
    public class AgentContext
    {
        public Guid SellerId { get; set; }
        public string UserMessage { get; set; } = string.Empty;
        public Conversation? CurrentConversation { get; set; }
        public Dictionary<string, object> ExtraData { get; set; } = new();
    }
}
