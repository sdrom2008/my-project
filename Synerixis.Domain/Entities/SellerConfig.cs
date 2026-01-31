using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Domain.Entities
{
    public class SellerConfig
    {
        public Guid Id { get; set; } = new Guid();
        public Guid SellerId { get; set; }          // FK to sellers.Id
        public string ShopName { get; set; }        // 店铺名称
        public string ShopLogo { get; set; }        // logo URL
        public string MainCategory { get; set; }    // 主营类目（如 "女装" "数码"）
        public string TargetCustomer { get; set; }  // 目标客户群体描述（文本）

        public string ReplyTone { get; set; }       // 回复语气：professional / friendly / humorous
        public string PreferredLanguage { get; set; } // zh / en / bilingual
        public bool AutoMarketingReminder { get; set; } = true; // 是否开启主动营销提醒
        public int MemoryRetentionDays { get; set; } = 180;     // 记忆保留天数，0=永久

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        public Seller Seller { get; set; }
    }
}
