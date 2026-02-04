using System;
using System.Collections.Generic;

namespace Synerixis.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? ExternalId { get; set; } = string.Empty;
        public string? ExternalSource { get; set; } = string.Empty;

        public string? Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string? ImagesJson { get; set; } = "[]";
        public string? VideosJson { get; set; } = "[]";
        //public string Category { get; set; } = string.Empty;  // 核心类目（全局）
        public string? TagsJson { get; set; } = "[]";

        public Guid CategoryId { get; set; }
        public Guid BrandId { get; set; }

        public string? AttributesJson { get; set; } = "{}";

        public int Status { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 导航集合（必须有）
        public ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
        public ICollection<SKU> SKUs { get; set; } = new List<SKU>();
        public ICollection<SellerProduct> SellerProducts { get; set; } = new List<SellerProduct>();

        // 新增：实体导航属性（必须加！）
        public Category? Category { get; set; }  // ← 这行就是报错的根源缺失
        public Brand? Brand { get; set; }
    }
}
