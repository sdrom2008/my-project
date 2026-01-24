using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Application.DTOs
{
    // 图片 Prompt 结果（未来扩展用）
    public class ImagePromptResult
    {
        public List<string> prompts { get; set; } = new();
        public string? suggestion { get; set; }
    }
}
