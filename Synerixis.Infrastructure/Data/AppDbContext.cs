// Synerixis.Infrastructure/Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using Synerixis.Application.DTOs;
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
        public DbSet<PayOrder> PayOrders { get; set; }                 // 支付订单表

        public DbSet<Product> Products { get; set; }
        public DbSet<SKU> SKUs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }

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

            // 统一所有 string 字段为 longtext（避免默认 varchar(255) 限制）
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

            // 显式配置表名和外键（避免大小写问题）
            modelBuilder.Entity<Seller>(entity =>
            {
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
                entity.HasKey(c => c.Id);
                entity.HasOne(c => c.Seller)
                      .WithMany(s => s.Conversations)
                      .HasForeignKey(c => c.SellerId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(c => c.SellerId);
            });

            // SellerConfig 配置
            modelBuilder.Entity<SellerConfig>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.Seller)
                      .WithOne(s => s.Config)  // 反向导航：Seller.Config
                      .HasForeignKey<SellerConfig>(c => c.SellerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SellerProduct 配置
            modelBuilder.Entity<SellerProduct>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.HasOne(p => p.Seller)
                      .WithMany(s => s.SellerProducts)  // Seller.SellerProducts 集合
                      .HasForeignKey(p => p.SellerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Product)
                      .WithMany(prod => prod.SellerProducts)  // Product.SellerProducts 集合
                      .HasForeignKey(p => p.ProductId)
                      .OnDelete(DeleteBehavior.NoAction);  // 防止级联删除商品本体
            });

            // Product 配置
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImagesJson).HasColumnType("nvarchar(max)");
                entity.Property(e => e.TagsJson).HasColumnType("nvarchar(max)");

                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Brand)
                      .WithMany(b => b.Products)
                      .HasForeignKey(e => e.BrandId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ProductAttribute 配置（改名后）
            modelBuilder.Entity<ProductAttribute>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Attributes)  // Product.Attributes 集合
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SKU 配置
            modelBuilder.Entity<SKU>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.SpecsJson).HasColumnType("nvarchar(max)");

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.SKUs)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Category 配置（树形）
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasMany(e => e.Products)
                      .WithOne(p => p.Category)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Brand 配置
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasMany(e => e.Products)
                      .WithOne(p => p.Brand)
                      .HasForeignKey(p => p.BrandId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // PayOrder 配置
            modelBuilder.Entity<PayOrder>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne<Seller>()
                      .WithMany()
                      .HasForeignKey(e => e.SellerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}
