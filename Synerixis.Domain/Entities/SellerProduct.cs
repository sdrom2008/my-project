using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Domain.Entities
{
    public class SellerProduct
    {
        public Guid Id { get; set; } = new Guid();
        public Guid SellerId { get; set; }

        public string ExternalProductId { get; set; }   // 淘宝/京东/拼多多商品ID
        public string Title { get; set; }               // 商品标题
        public string Description { get; set; }         // 原始描述
        public decimal Price { get; set; }
        public string ImagesJson { get; set; }          // JSON 数组：["url1", "url2"]
        public string Category { get; set; }
        public string TagsJson { get; set; }            // JSON 数组：["爆款","高性价比"]

        public string ImportedFrom { get; set; }        // taobao / jd / pdd / manual / excel
        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        public Seller Seller { get; set; }
    }
}
