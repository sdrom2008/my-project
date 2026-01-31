// Synerixis.Infrastructure/Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using Synerixis.Domain.Entities;

namespace Synerixis.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<SellerConfig> SellerConfigs { get; set; }
        public DbSet<SellerProduct> SellerProducts { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 统一所有 Guid 字段为 binary(16)
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(Guid) || property.ClrType == typeof(Guid?))
                    {
                        property.SetColumnType("binary(16)");
                    }
                }
            }

            // 显式配置表名和外键（避免大小写问题）
            modelBuilder.Entity<Seller>(entity =>
            {
                entity.ToTable("sellers");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.OpenId).HasMaxLength(128).IsRequired();
                entity.HasIndex(s => s.OpenId).IsUnique();
            });

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.ToTable("conversations");
                entity.HasKey(c => c.Id);
                entity.HasOne(c => c.Seller)
                      .WithMany(s => s.Conversations)
                      .HasForeignKey(c => c.SellerId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(c => c.SellerId);
            });

            modelBuilder.Entity<SellerConfig>()
                .HasOne(c => c.Seller)
                .WithOne()
                .HasForeignKey<SellerConfig>(c => c.SellerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SellerProduct>()
                .HasOne(p => p.Seller)
                .WithMany()
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
