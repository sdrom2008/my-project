using Synerixis.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Domain.Entities
{
    public class Seller : AggregateRoot<Guid>
    {
        public string OpenId { get; private set; } = string.Empty;          // 微信 OpenId，唯一标识
        public string? Username { get; private set; }                       // 可选用户名
        public string? Email { get; private set; }                          // 可选邮箱
        public string? PasswordHash { get; private set; }                   // 可选本地登录用
        public string? Nickname { get; private set; }                       // 昵称（微信返回或手动设置）
        public string? AvatarUrl { get; private set; }                      // 头像
        public bool IsActive { get; private set; } = true;                  // 是否激活
        public string SubscriptionLevel { get; private set; } = "Free";    // Free / Basic / Pro
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // 反向导航：该商家的所有会话
        public List<Conversation> Conversations { get; private set; } = new();

        private Seller() { }  // EF Core 无参构造

        public static Seller Create(string openId, string? nickname = null, string? avatarUrl = null)
        {
            return new Seller
            {
                Id = Guid.NewGuid(),
                OpenId = openId,
                Nickname = nickname,
                AvatarUrl = avatarUrl
            };
        }

        public void UpdateProfile(string? nickname, string? avatarUrl)
        {
            Nickname = nickname ?? Nickname;
            AvatarUrl = avatarUrl ?? AvatarUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RecordLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpgradeSubscription(string newLevel)
        {
            if (!new[] { "Free", "Basic", "Pro" }.Contains(newLevel))
                throw new ArgumentException("无效的订阅等级");

            SubscriptionLevel = newLevel;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
