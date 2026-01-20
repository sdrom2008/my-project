using MyProject.Domain.Common;
using MyProject.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyProject.Domain;

namespace MyProject.Domain.Entities
{
    public class Product : AggregateRoot<Guid>
    {
        public Guid ShopId { get; private set; }
        public string OriginalTitle { get; private set; }
        public string OriginalDescription { get; private set; }
        public List<string> OriginalImageUrls { get; private set; } = new();

        public string OptimizedTitle { get; private set; }
        public string OptimizedDescription { get; private set; } // Markdown 或富文本
        public List<OptimizedImage> OptimizedImages { get; private set; } = new();

        public MarketingPlan MarketingPlan { get; private set; }

        public OptimizationStatus Status { get; private set; } = OptimizationStatus.Pending;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; private set; }

        // ... 其他属性如 Category, PlatformTarget (Taobao/Pinduoduo/Douyin等)
    }
}
