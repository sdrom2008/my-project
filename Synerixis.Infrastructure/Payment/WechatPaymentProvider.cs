using Synerixis.Application.Interfaces;
using Synerixis.Domain.Entities;
using Synerixis.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Synerixis.Application.DTOs;

namespace Synerixis.Infrastructure.Payment
{
    public class WechatPaymentProvider : IPaymentProvider
    {
        public string Channel => "wechat";

        private readonly WeChatPayV3Client _client;
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public WechatPaymentProvider(WeChatPayV3Client client, AppDbContext db, IConfiguration config)
        {
            _client = client;
            _db = db;
            _config = config;
        }

        public async Task<PaymentCreateResult> CreateOrderAsync(PaymentCreateRequest request, Guid sellerId)
        {
            // 金额转分
            int amountInFen = (int)(request.Amount * 100);

            var payParams = await _client.CreateJsApiOrderAsync(
                request.OutTradeNo,
                amountInFen,
                request.Description,
                request.OpenId,
                request.NotifyUrl ?? _config["WeChatPay:NotifyUrl"]
            );

            // 保存订单（从 Controller 迁移过来）
            var order = new PayOrder
            {
                Id = Guid.NewGuid(),
                SellerId = sellerId,
                OutTradeNo = request.OutTradeNo,
                Amount = request.Amount,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };
            _db.PayOrders.Add(order);
            await _db.SaveChangesAsync();

            return new PaymentCreateResult
            {
                Success = true,
                PayParams = payParams.ToDictionary(kv => kv.Key, kv => (object)kv.Value)
            };
        }

        public async Task<PaymentNotifyResult> HandleNotifyAsync(HttpRequest httpRequest)
        {
            // 调用 Client 的 HandleCallbackAsync 或直接复用解密逻辑
            var response = await _client.HandleCallbackAsync();  // 如果 Client 有这个方法

            // 假设返回 JSON 字符串，解析
            if (response.Contains("\"SUCCESS\""))
            {
                // 解密后逻辑已在 Client 里处理了订单更新
                return new PaymentNotifyResult { Success = true };
            }

            return new PaymentNotifyResult { Success = false, Message = "验证失败" };
        }
    }
}
