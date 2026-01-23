using MyProject.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyProject.Domain.Entities
{
    public class Conversation : AggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
        public string Title { get; private set; } = "新对话";
        public List<ChatMessage> Messages { get; private set; } = new();
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? LastActiveAt { get; private set; }

        // 外键：关联 Seller（商家）
        public Guid SellerId { get; private set; }

        // 导航属性（可选，但推荐加，便于查询）
        public Seller? Seller { get; private set; }

        // 会话唯一标识（如果你需要一个字符串形式的 ID，比如前端用）
        public string ConversationId { get; private set; } = string.Empty;  // 新增这个

        // 或者如果你想在数据库里用 JSON 列存储 Messages（不推荐长期用，建议用单独表）
        // public string MessagesJson { get; private set; } = "[]";

        private Conversation() { }

        public static Conversation Create(Guid userId)
        {
            return new Conversation { Id = Guid.NewGuid(), UserId = userId };
        }

        public static Conversation Create(Guid sellerId, string? title = null)
        {
            return new Conversation
            {
                Id = Guid.NewGuid(),
                SellerId = sellerId,
                ConversationId = Guid.NewGuid().ToString("N"),  // 生成唯一字符串 ID
                Title = title ?? "新对话"
            };
        }

        public void AddMessage(ChatMessage message)
        {
            Messages.Add(message);
            LastActiveAt = DateTime.UtcNow;
        }
    }

    public class ChatMessage
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public bool IsFromUser { get; private set; }
        public string Content { get; private set; } = string.Empty;
        public string? Type { get; private set; }  // "text", "optimize_result" 等
        public string? DataJson { get; private set; }  // 结构化结果存 JSON
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

        private ChatMessage() { }

        public static ChatMessage FromUser(string content)
        {
            return new ChatMessage { IsFromUser = true, Content = content };
        }

        public static ChatMessage FromAI(string content, string? type = "text", object? data = null)
        {
            return new ChatMessage
            {
                IsFromUser = false,
                Content = content,
                Type = type,
                DataJson = data != null ? JsonSerializer.Serialize(data) : null
            };
        }
    }
}
