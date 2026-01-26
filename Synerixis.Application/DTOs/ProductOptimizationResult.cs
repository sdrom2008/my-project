using Synerixis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.DTOs
{
    // 商品优化结果
    public class ProductOptimizationResult
    {
        public string optimizedTitle { get; set; } = string.Empty;
        public string optimizedDescription { get; set; } = string.Empty;  // 支持 Markdown
        public MarketingPlan marketingPlan { get; set; } = new();
        public List<string> imagePrompts { get; set; } = new();
    }
}
