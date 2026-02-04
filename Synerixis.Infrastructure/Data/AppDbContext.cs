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
        public DbSet<PayOrder> PayOrders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<SKU> SKUs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. 统一 Guid 为 binary(16)（放在最前面）
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

            // 2. 所有 string 字段默认 longtext（MySQL 兼容）
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(string) && property.GetMaxLength() == null)
                    {
                        property.SetColumnType("longtext");
                    }
                }
            }

            // 3. 显式配置每个实体（表名 + 关系）
            modelBuilder.Entity<Seller>(entity =>
            {
                entity.ToTable("sellers");
                entity.HasKey(s => s.Id);

                // 明确指定 OpenId 类型和长度（防止被全局 longtext 覆盖）
                entity.Property(s => s.OpenId)
                      .HasColumnType("varchar(128)")
                      .HasMaxLength(128)
                      .IsRequired();

                entity.HasIndex(s => s.OpenId)
                      .IsUnique()
                      .HasDatabaseName("IX_sellers_OpenId");
            });

            modelBuilder.Entity<SellerConfig>(entity =>
            {
                entity.ToTable("seller_configs");
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.Seller)
                      .WithOne(s => s.Config)
                      .HasForeignKey<SellerConfig>(c => c.SellerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SellerProduct>(entity =>
            {
                entity.ToTable("seller_products");
                entity.HasKey(p => p.Id);

                entity.HasOne(p => p.Seller)
                      .WithMany(s => s.SellerProducts)
                      .HasForeignKey(p => p.SellerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Product)
                      .WithMany(prod => prod.SellerProducts)
                      .HasForeignKey(p => p.ProductId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Brand)
                      .WithMany(b => b.Products)
                      .HasForeignKey(e => e.BrandId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ProductAttribute>(entity =>
            {
                entity.ToTable("product_attributes");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Attributes)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SKU>(entity =>
            {
                entity.ToTable("skus");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.SKUs)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(e => e.Id);

                entity.HasMany(e => e.Products)
                      .WithOne(p => p.Category)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Brand>(entity =>
            {
                entity.ToTable("brands");
                entity.HasKey(e => e.Id);

                entity.HasMany(e => e.Products)
                      .WithOne(p => p.Brand)
                      .HasForeignKey(p => p.BrandId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PayOrder>(entity =>
            {
                entity.ToTable("pay_orders");
                entity.HasKey(e => e.Id);

                entity.HasOne<Seller>()
                      .WithMany()
                      .HasForeignKey(e => e.SellerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.ToTable("conversations");
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.Seller)
                      .WithMany(s => s.Conversations)
                      .HasForeignKey(c => c.SellerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.ToTable("chat_messages");
                entity.HasKey(m => m.Id);

                entity.HasOne(m => m.Conversation)
                      .WithMany(c => c.Messages)
                      .HasForeignKey(m => m.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}