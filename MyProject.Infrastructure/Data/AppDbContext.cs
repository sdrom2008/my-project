// MyProject.Infrastructure/Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Entities;

namespace MyProject.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<SellerConfig> SellerConfigs { get; set; }
        public DbSet<Conversation> Conversations { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 
            //  Seller 实体，也要配置
            modelBuilder.Entity<Seller>(entity =>
            {
                entity.ToTable("Sellers");
                entity.HasKey(s => s.Id);

                entity.Property(s => s.OpenId).HasMaxLength(128).IsRequired();
                entity.HasIndex(s => s.OpenId).IsUnique();

                entity.Property(s => s.Username).HasMaxLength(50);
                entity.Property(s => s.Email).HasMaxLength(100);
                entity.Property(s => s.PasswordHash).HasMaxLength(256);
                entity.Property(s => s.Nickname).HasMaxLength(100);
                entity.Property(s => s.AvatarUrl).HasMaxLength(500);
                entity.Property(s => s.SubscriptionLevel).HasMaxLength(20).HasDefaultValue("Free");

                entity.Property(s => s.IsActive).HasDefaultValue(true);

                entity.HasMany(s => s.Conversations)
                      .WithOne(c => c.Seller)
                      .HasForeignKey(c => c.SellerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SellerConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Seller).WithMany().HasForeignKey(e => e.SellerId);
                entity.Property(e => e.CustomRules).HasColumnType("json");
            });

            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(e => e.Id);  // 主键 Guid

                // 关联 Seller（导航属性 + 外键）
                entity.HasOne(e => e.Seller)
                      .WithMany(s => s.Conversations)  // Seller 实体里要加 List<Conversation> Conversations
                      .HasForeignKey(e => e.SellerId)
                      .OnDelete(DeleteBehavior.Restrict);  // 避免级联删除

                // ConversationId 字符串字段（唯一、可索引）
                entity.Property(e => e.ConversationId)
                      .IsRequired()
                      .HasMaxLength(128)
                      .IsUnicode(false);  // 建议用 ASCII，避免中文乱码

                entity.HasIndex(e => e.ConversationId).IsUnique();  // 唯一索引

                // Messages 如果用单独表（推荐）
                entity.HasMany(e => e.Messages)
                      .WithOne()  // 如果 ChatMessage 有 ConversationId 外键
                      .HasForeignKey("ConversationId")
                      .OnDelete(DeleteBehavior.Cascade);

                // 如果你坚持用 JSON 列存储 Messages（不推荐，但可以临时用）
                // entity.Property(e => e.MessagesJson)
                //       .HasColumnType("json")
                //       .HasConversion(
                //           v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                //           v => JsonSerializer.Deserialize<List<ChatMessage>>(v, new JsonSerializerOptions()) ?? new()
                //       );
            });
        }
    }
}
