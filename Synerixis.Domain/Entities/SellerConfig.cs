using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Domain.Entities
{
    public class SellerConfig
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SellerId { get; set; }
        public Seller? Seller { get; set; }  // 可选导航属性

        public string? ApiKey { get; set; }              // 通义千问 API Key
        public string? DbConnectionString { get; set; }  // 如果每个卖家独立数据库连接（可选）
        public string? CustomRules { get; set; }         // JSON 格式的自定义意图规则，例如 {"orderQuery": "select * from orders where ..."}
    }
}
