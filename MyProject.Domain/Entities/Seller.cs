using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Domain.Entities
{
    public class Seller
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OpenId { get; set; } = string.Empty;          // 微信 openid，唯一标识
        public string? Name { get; set; }                           // 卖家昵称，可后期完善
        public string SubscriptionLevel { get; set; } = "Free";    // Free / Basic / Pro
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // 可选导航属性（如果后续需要双向关联）
        // public virtual ICollection<SellerConfig> Configs { get; set; } = new List<SellerConfig>();
        // public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    }
}
