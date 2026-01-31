using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Synerixis.Application.DTOs;
using Synerixis.Application.Interfaces;
using Synerixis.Domain.Entities;
using Synerixis.Infrastructure.Data;
using System.Security.Claims;

namespace Synerixis.Api.Controllers
{
    [ApiController]
    [Route("api/seller")]
    [Authorize]  // 需要 JWT 认证
    public class SellerController : BaseApiController
    {
        private readonly AppDbContext _db;
        private readonly IAuthService _authService;  // 如果需要

        public SellerController(AppDbContext db, IAuthService authService)
        {
            _db = db;
            _authService = authService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var sellerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

            var seller = await _db.Sellers
                .Include(s => s.Config)
                .FirstOrDefaultAsync(s => s.Id == sellerId);

            if (seller == null) return NotFound("商户不存在");

            var result = new
            {
                seller.Id,
                seller.Phone,
                seller.OpenId,
                seller.FreeQuota,
                seller.SubscriptionEnd,
                seller.LastLoginType,
                Config = seller.Config ?? new SellerConfig { SellerId = sellerId }
            };

            return Ok(result);
        }

        [HttpPut("config")]
        public async Task<IActionResult> UpdateConfig([FromBody] SellerConfigUpdateDto dto)
        {
            var sellerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

            var config = await _db.SellerConfigs.FirstOrDefaultAsync(c => c.SellerId == sellerId);

            if (config == null)
            {
                config = new SellerConfig
                {
                    SellerId = sellerId,
                    DefaultReplyTone = dto.DefaultReplyTone ?? "professional",
                    PreferredLanguage = dto.PreferredLanguage ?? "zh",
                    MemoryRetentionDays = dto.MemoryRetentionDays > 0 ? dto.MemoryRetentionDays : 180
                };
                _db.SellerConfigs.Add(config);
            }
            else
            {
                config.ShopName = dto.ShopName ?? config.ShopName;
                config.ShopLogo = dto.ShopLogo ?? config.ShopLogo;
                config.MainCategory = dto.MainCategory ?? config.MainCategory;
                config.TargetCustomerDesc = dto.TargetCustomerDesc ?? config.TargetCustomerDesc;
                config.DefaultReplyTone = dto.DefaultReplyTone ?? config.DefaultReplyTone;
                config.PreferredLanguage = dto.PreferredLanguage ?? config.PreferredLanguage;
                config.EnableAutoMarketingReminder = dto.EnableAutoMarketingReminder;
                config.MemoryRetentionDays = dto.MemoryRetentionDays > 0 ? dto.MemoryRetentionDays : config.MemoryRetentionDays;
                config.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            return Ok(new { msg = "配置更新成功" });
        }
    }
}
