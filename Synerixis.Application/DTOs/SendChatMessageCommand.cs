using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Synerixis.Application.DTOs
{
    public class SendChatMessageCommand
    {
        [JsonPropertyName("conversationId")]
        public Guid? ConversationId { get; set; }       // guid

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty; // 用户输入的消息

        public Dictionary<string, string>? ExtraData { get; set; } // 扩展：商品标题、描述、类目等
    }
}
