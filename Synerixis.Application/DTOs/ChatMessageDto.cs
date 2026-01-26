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
        public string? Type { get; set; }
        public object? Data { get; set; }  // 反序列化后的结构化数据
    }
}
