using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Application.DTOs
{
    // 营销文案结果（未来扩展用）
    public class MarketingCopyResult
    {
        public string shortVideoScript { get; set; } = string.Empty;
        public string plantingText { get; set; } = string.Empty;
        public string liveScript { get; set; } = string.Empty;
        public List<string> keySellingPoints { get; set; } = new();
    }
}
