using System;

namespace Synerixis.Domain.Entities
{
    public class SKU
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }

        public string SpecsJson { get; set; } = "{}";  // 规格JSON（如 {"颜色": "红", "尺寸": "M"}）

        public decimal Price { get; set; }
        public int Stock { get; set; }

        public string ExternalCode { get; set; } = string.Empty;  // 外部 SKU 码

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 导航
        public Product Product { get; set; }
    }
}