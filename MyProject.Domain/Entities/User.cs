using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyProject.Domain.Common;

namespace MyProject.Domain.Entities
{
    public class User : AggregateRoot<Guid>  // 假设你有 AggregateRoot 基类，如果没有可以先用普通 class
    {
        public string Username { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;  // 后续用 Identity 或 BCrypt
        public string? Nickname { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; private set; }

        // 私有构造，推荐领域驱动风格
        private User() { }  // EF Core 需要无参构造

        public static User Create(string username, string email, string passwordHash, string? nickname = null)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                Nickname = nickname
            };
        }

        // 后续可加修改方法，如 UpdateNickname 等
    }
}
