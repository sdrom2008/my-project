using System;
using System.Collections.Generic;

namespace Synerixis.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        public Guid? ParentId { get; set; }  // 父类目

        public bool IsLeaf { get; set; } = true;  // 是否叶子节点

        public int SortOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 导航
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}