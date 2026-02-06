using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Senparc.CO2NET.Extensions;
using Synerixis.Application.DTOs;
using Synerixis.Application.Interfaces;
using Synerixis.Application.Services;
using Synerixis.Domain.Entities;
using Synerixis.Infrastructure.Data;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace Synerixis.Api.Controllers
{
    [ApiController]
    [Route("api/seller")]
    [Authorize]  // 需要 JWT 认证
    public class SellerController : BaseApiController
    {
        private readonly AppDbContext _db;
        private readonly IAuthService _authService;  // 如果需要
        private readonly ProductService _productService;

        public SellerController(AppDbContext db, IAuthService authService, ProductService productService)
        {
            _db = db;
            _authService = authService;
            _productService = productService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var sellerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(sellerIdStr, out var sellerId))
                    return Unauthorized("无效身份");

                var seller = await _db.Sellers
                    .Include(s => s.Config)
                    .FirstOrDefaultAsync(s => s.Id == sellerId);

                if (seller == null)
                    return NotFound("商户不存在");

                if (seller.Config == null)
                {
                    seller.Config = new SellerConfig { /* 默认值 */ };
                    _db.SellerConfigs.Add(seller.Config);
                    await _db.SaveChangesAsync();
                }

                return Ok(new
                {
                    seller.Id,
                    seller.Nickname,
                    seller.AvatarUrl,
                    seller.Phone,
                    seller.FreeQuota,
                    seller.SubscriptionLevel,
                    seller.SubscriptionEnd,
                    Config = seller.Config
                });
            }
            catch (Exception ex)
            {
                // 加日志
                Console.WriteLine("GetProfile 异常: " + ex.Message);
                return StatusCode(500, "服务器内部错误，请检查日志");
            }
        }

        [HttpPut("config")]
        public async Task<IActionResult> UpdateConfig([FromBody] SellerConfigUpdateDto dto)
        {
            var sellerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(sellerIdStr, out var sellerId))
                return Unauthorized();

            var config = await _db.SellerConfigs.FirstOrDefaultAsync(c => c.SellerId == sellerId);

            if (config == null)
                return NotFound("配置不存在");

            config.ShopName = dto.ShopName ?? config.ShopName;
            config.ShopLogo = dto.ShopLogo ?? config.ShopLogo;
            config.MainCategory = dto.MainCategory ?? config.MainCategory;
            config.TargetCustomerDesc = dto.TargetCustomerDesc ?? config.TargetCustomerDesc;
            config.DefaultReplyTone = dto.DefaultReplyTone ?? config.DefaultReplyTone;
            config.PreferredLanguage = dto.PreferredLanguage ?? config.PreferredLanguage;
            config.EnableAutoMarketingReminder = dto.EnableAutoMarketingReminder;
            config.MemoryRetentionDays = dto.MemoryRetentionDays > 0 ? dto.MemoryRetentionDays : config.MemoryRetentionDays;
            config.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { message = "配置更新成功" });
        }

        // 更新个人信息（昵称、头像等）
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto dto)
        {
            var sellerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(sellerIdStr, out var sellerId))
                return Unauthorized();

            var seller = await _db.Sellers.FirstOrDefaultAsync(s => s.Id == sellerId);

            if (seller == null)
                return NotFound("商户不存在");

            seller.UpdateProfile(dto.Nickname, dto.AvatarUrl);
            await _db.SaveChangesAsync();

            return Ok(new { message = "个人信息更新成功" });
        }

        [HttpPost("upload/logo")]
        [Authorize]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("请选择文件");

            var sellerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(sellerIdStr, out var sellerId))
                return Unauthorized();

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(extension))
                return BadRequest("仅支持 JPG/PNG/GIF 格式");

            var fileName = $"{Guid.NewGuid()}{extension}";
            var dir = Path.Combine("wwwroot", "uploads", "logo");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/logo/{fileName}";
            return Ok(new { url });
        }



        // 统一商品列表接口（支持分页 + 搜索）
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string keyword = null)
        {
            try
            {
                var sellerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(sellerIdStr, out var sellerId))
                    return Unauthorized("无效的身份令牌");

                var products = await _productService.GetProductsAsync(sellerId, page, pageSize, keyword);

                var items = products.Select(p => new
                {
                    id = p.Id,
                    title = p.Product?.Title ?? "未命名商品",
                    price = p.CustomPrice ?? p.Product?.Price ?? 0,
                    category = p.Product?.Category?.Name ?? "未分类",
                    imagesJson = p.Product?.ImagesJson ?? "[]",
                    tagsJson = p.Product?.TagsJson ?? "[]",
                    source = p.Source ?? "manual",
                    importedAt = p.ImportedAt,
                    optimizedTitle = p.OptimizedTitle,
                    optimizedDescription = p.OptimizedDescription,
                    optimizedTagsJson = p.OptimizedTagsJson
                }).ToList();

                var total = await _db.SellerProducts.CountAsync(p => p.SellerId == sellerId);

                return Ok(new { total, page, pageSize, items });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetProducts 异常: {ex.Message}");
                return StatusCode(500, new { message = "获取商品列表失败", error = ex.Message });
            }
        }

        // 删除商品（调用 Service 层更好，但这里保持简单）
        [HttpDelete("products/{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var sellerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(sellerIdStr, out var sellerId))
                return Unauthorized();

            var product = await _db.SellerProducts.FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId);
            if (product == null)
                return NotFound("商品不存在或无权限");

            _db.SellerProducts.Remove(product);
            await _db.SaveChangesAsync();

            return Ok(new { message = "删除成功" });
        }






        // AI 优化（保存优化结果）
        [HttpPut("products/{id}/optimize")]
        public async Task<IActionResult> OptimizeProduct(Guid id, [FromBody] OptimizedProductDto dto)
        {
            var sellerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(sellerIdStr, out var sellerId))
                return Unauthorized();

            var sellerProduct = await _db.SellerProducts.FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId);
            if (sellerProduct == null)
                return NotFound("商品不存在或无权限");

            // 假设 SellerProduct 有这些字段（需加到实体类）
            sellerProduct.OptimizedTitle = dto.OptimizedTitle;
            sellerProduct.OptimizedDescription = dto.OptimizedDescription;
            sellerProduct.OptimizedTagsJson = dto.OptimizedTagsJson;
            sellerProduct.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { message = "优化结果保存成功" });
        }

        [HttpPost("products/fetch-url")]
        public async Task<IActionResult> FetchFromUrl([FromBody] FetchUrlDto dto)
        {
            var sellerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(sellerIdStr, out var sellerId))
                return Unauthorized();

            // 模拟抓取逻辑（实际可加爬虫）
            var product = new
            {
                title = "抓取商品 - 示例",
                description = "抓取描述",
                price = 99.99,
                imagesJson = "[]",
                category = "测试类目",
                tagsJson = "[]"
            };

            return Ok(product);
        }

        [HttpPost("products/import")]
        public async Task<IActionResult> ImportProduct([FromBody] ProductImportDto dto)
        {
            try
            {
                var sellerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(sellerIdStr, out var sellerId))
                    return Unauthorized("无效的身份令牌");

                if (dto == null)
                    return BadRequest("请求体为空");

                if (string.IsNullOrWhiteSpace(dto.Title))
                    return BadRequest("商品标题不能为空");

                // 设置默认值
                dto.Description ??= "";
                dto.ImagesJson ??= "[]";
                dto.TagsJson ??= "[]";
                dto.Category ??= "";
                dto.Source ??= "manual";

                var productId = await _productService.ImportProductAsync(sellerId, dto);

                return Ok(new { message = "商品导入成功", productId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ImportProduct 异常: {ex.Message}");
                return StatusCode(500, new { message = "商品导入失败", error = ex.Message });
            }
        }

        //测试
        //[HttpPost("products/import")]
        //public IActionResult ImportProduct([FromBody] TestDto dto)
        //{
        //    Console.WriteLine("[TEST] 进入方法，Title: " + dto?.Title + ", Desc: " + dto?.Description + ",Price:" +dto.Price.ToString());
        //    return Ok(new { message = "收到" });
        //}
    }

    public class FetchUrlDto
    {
        public string Url { get; set; }
    }

    //测试用
    public class TestDto
    {
        public string? ExternalId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }  // string
        public string? ImagesJson { get; set; }
        public string? Category { get; set; }
        public string? TagsJson { get; set; }
        //public string Source { get; set; }
    }
}
