using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Domain.Entities
{
    public class SellerProduct
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? SellerId { get; set; }
        public Guid? ProductId { get; set; }

        public decimal? CustomPrice { get; set; }  // 商户自定义价格
        public int? CustomStock { get; set; }      // 自定义库存

        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? Source { get; set; } = "manual";  // 类型：string，值如 "manual"、"taobao"

        // 商户专属优化结果（核心：加在这里）
        public string? OptimizedTitle { get; set; } = string.Empty;
        public string? OptimizedDescription { get; set; } = string.Empty;
        public string? OptimizedTagsJson { get; set; } = "[]";

        // 导航属性
        public Seller Seller { get; set; }
        public Product Product { get; set; }


    }
}
