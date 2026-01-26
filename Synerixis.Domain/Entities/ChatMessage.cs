using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Synerixis.Domain.Entities
{
    public class ChatMessage
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid ConversationId { get; private set; }  // 外键，必须保留
        public Conversation? Conversation { get; private set; }  // 导航，可选

        public bool IsFromUser { get; private set; }
        public string Content { get; private set; } = string.Empty;

        // 推荐改成 MessageType（更规范，与 DTO 一致）
        public string MessageType { get; private set; } = "text";  // 改名：Type → MessageType

        public string? DataJson { get; private set; }
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

        private ChatMessage() { }

        public static ChatMessage FromUser(string content)
        {
            return new ChatMessage
            {
                IsFromUser = true,
                Content = content,
                MessageType = "text"  // 显式设置
            };
        }

        public static ChatMessage FromAI(string content, string messageType = "text", object? data = null)
        {
            return new ChatMessage
            {
                IsFromUser = false,
                Content = content,
                MessageType = messageType,
                DataJson = data != null ? System.Text.Json.JsonSerializer.Serialize(data) : null
            };
        }

        public static ChatMessage FromAI(string content, string messageType = "text", object? data = null, DateTime? timestamp = null)
        {
            var msg = new ChatMessage
            {
                IsFromUser = false,
                Content = content,
                MessageType = messageType,
                DataJson = data != null ? JsonConvert.SerializeObject(data) : null, // 改用 Newtonsoft
                Timestamp = timestamp ?? DateTime.UtcNow
            };
            return msg;
        }
    }
}
