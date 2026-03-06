using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Synerixis.Application.DTOs;
using Synerixis.Application.Interfaces;
using Synerixis.Domain.Entities;
using Synerixis.Infrastructure.Data;
using System;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Synerixis.Api.Controllers
{
    [ApiController]
    [Route("api/pay")]
    [Authorize]
    public class PayController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IPaymentProviderFactory _factory;

        public PayController(AppDbContext db, IConfiguration config, IPaymentProviderFactory factory)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        // 统一创建支付接口（支持微信/支付宝）
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] PaymentCreateRequest request)
        {
            if (string.IsNullOrEmpty(request.Channel))
                return BadRequest("缺少支付渠道 (wechat/alipay)");

            var sellerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(sellerIdStr, out var sellerId))
                return Unauthorized("无效身份");

            var seller = await _db.Sellers.FirstOrDefaultAsync(s => s.Id == sellerId);
            if (seller == null)
                return NotFound("商户不存在");

            var provider = _factory.GetProvider(request.Channel);
            if (provider == null)
                return BadRequest($"不支持渠道: {request.Channel}");

            // 微信需要 OpenId
            if (request.Channel == "wechat" && string.IsNullOrEmpty(seller.OpenId))
                return BadRequest("请先绑定微信");

            var result = await provider.CreateOrderAsync(request, sellerId);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }

        // 统一回调接口
        [HttpPost("notify/{channel}")]
        [AllowAnonymous]
        public async Task<IActionResult> Notify(string channel)
        {
            var provider = _factory.GetProvider(channel);
            if (provider == null)
                return BadRequest("不支持渠道");

            var notifyResult = await provider.HandleNotifyAsync(Request);

            if (notifyResult.Success)
            {
                var order = await _db.PayOrders.FirstOrDefaultAsync(o => o.OutTradeNo == notifyResult.OutTradeNo);
                if (order == null || order.Status == "paid")
                    return Content("success");

                order.Status = "paid";
                order.TransactionId = notifyResult.TransactionId;
                order.PaidAt = DateTime.UtcNow;

                var seller = await _db.Sellers.FirstOrDefaultAsync(s => s.Id == order.SellerId);
                if (seller != null)
                {
                    seller.ApplySubscription(order.Amount);  // 订阅升级
                }

                await _db.SaveChangesAsync();

                return Content("success");
            }

            return Content("fail");
        }

    }

 
}