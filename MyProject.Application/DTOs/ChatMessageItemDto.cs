using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Application.DTOs
{
    public class ChatMessageItemDto
    {
        public bool IsFromUser { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? MessageType { get; set; }  // "text", "optimize_result", "image_prompts" 等
        public object? Data { get; set; }         // 结构化数据（如优化结果对象）
    }
}
