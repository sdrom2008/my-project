using MyProject.Domain.Common;

namespace MyProject.Domain.Entities
{
    public class User : AggregateRoot<Guid>  // 假设你有 AggregateRoot 基类，如果没有可以先用普通 class
    {
        public string Username { get; private set; } = string.Empty;
        public string OpenId { get; private set; } = string.Empty;  // 微信等第三方登录标识
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;  // 后续用 Identity 或 BCrypt
        public string? Nickname { get; private set; }
        public bool IsActive { get; private set; } = true;
        public string? AvatarUrl { get; private set; }      
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; private set; }
        public string SubscriptionLe { get; private set; } = "Free";  // Free/Basic/Pro

        // 私有构造，推荐领域驱动风格
        private User() { }  // EF Core 需要无参构造

        public static User Create(string openId, string? nickname = null, string? avatar = null)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                OpenId = openId,
                Nickname = nickname,
                AvatarUrl = avatar
            };
        }

        //public static User Create(string username,string openId, string email, string passwordHash, string? nickname = null)
        //{
        //    return new User
        //    {
        //        Id = Guid.NewGuid(),
        //        OpenId = openId,
        //        Username = username,
        //        Email = email,
        //        PasswordHash = passwordHash,
        //        Nickname = nickname
        //    };
        //}

        // 后续可加修改方法，如 UpdateNickname 等
        public void UpdateProfile(string? nickname, string? avatar)
        {
            Nickname = nickname ?? Nickname;
            AvatarUrl = avatar ?? AvatarUrl;
        }
    }
}
