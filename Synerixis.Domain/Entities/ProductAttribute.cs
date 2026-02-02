using System;

namespace Synerixis.Domain.Entities
{
    public class ProductAttribute
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }

        public string Name { get; set; } = string.Empty;   // 属性名，如 "颜色"
        public string Value { get; set; } = string.Empty;  // 属性值，如 "红色"

        public bool IsKey { get; set; } = false;    // 是否关键属性（用于 SPU）
        public bool IsSale { get; set; } = false;   // 是否销售属性（用于生成 SKU）

        public Guid? ParentId { get; set; }  // 支持属性层级

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Product Product { get; set; }
    }
}