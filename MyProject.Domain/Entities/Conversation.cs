using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Domain.Entities
{
    public class Conversation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SellerId { get; set; }
        public Seller? Seller { get; set; }  // 可选导航属性

        public string ConversationId { get; set; } = string.Empty;  // 前端/后端使用的会话ID（唯一字符串）
        public string? Messages { get; set; }                       // JSON 序列化的消息历史数组
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public int MessageCount { get; set; } = 0;
        public double? SatisfactionScore { get; set; }              // 买家结束对话时的满意度评分（0-5）
    }
}
