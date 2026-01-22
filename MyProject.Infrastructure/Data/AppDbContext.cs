// MyProject.Infrastructure/Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Entities;

namespace MyProject.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<SellerConfig> SellerConfigs { get; set; }
        public DbSet<Conversation> Conversations { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User 表配置（示例）
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.OpenId).IsUnique();   // OpenId 唯一
                entity.Property(u => u.OpenId).HasMaxLength(128).IsRequired();
                entity.Property(u => u.Username).HasMaxLength(50);
                entity.Property(u => u.Email).HasMaxLength(100);
                entity.Property(u => u.PasswordHash).HasMaxLength(256);
                entity.Property(u => u.Nickname).HasMaxLength(100);
                entity.Property(u => u.AvatarUrl).HasMaxLength(500);
                entity.Property(u => u.SubscriptionLe).HasMaxLength(20);
            });


            // 后续加其他实体配置...
            modelBuilder.Entity<Seller>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OpenId).IsUnique();
                entity.Property(e => e.OpenId).IsRequired().HasMaxLength(128);
                entity.Property(e => e.SubscriptionLevel).HasMaxLength(50);
            });

            modelBuilder.Entity<SellerConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Seller).WithMany().HasForeignKey(e => e.SellerId);
                entity.Property(e => e.CustomRules).HasColumnType("json");
            });

            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Seller).WithMany().HasForeignKey(e => e.SellerId);
                entity.Property(e => e.ConversationId).IsRequired().HasMaxLength(128);
                entity.Property(e => e.Messages).HasColumnType("json");
            });
        }
    }
}
