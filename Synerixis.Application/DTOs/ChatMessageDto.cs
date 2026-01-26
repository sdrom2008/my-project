using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.DTOs
{
    public class ChatMessageDto
    {
        public bool IsFromUser { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "text";  // text, order_card, product_opt, image, etc.
        public object? Data { get; set; }                  // 结构化数据
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
