using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public PayController(AppDbContext db, IConfiguration config)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        private string AppId => _config["WeChatPay:AppId"] ?? throw new InvalidOperationException("缺少 AppId");
        private string MchId => _config["WeChatPay:MchId"] ?? throw new InvalidOperationException("缺少 MchId");
        private string ApiV3Key => _config["WeChatPay:ApiV3Key"] ?? throw new InvalidOperationException("缺少 ApiV3Key");
        private string CertPath => _config["WeChatPay:CertPath"] ?? throw new InvalidOperationException("缺少 CertPath");
        private string CertPassword => _config["WeChatPay:CertPassword"] ?? throw new InvalidOperationException("缺少 CertPassword");
        private string NotifyUrl => _config["WeChatPay:NotifyUrl"] ?? "https://your-domain.com/api/pay/callback";

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder()
        {
            var sellerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(sellerIdStr, out var sellerId))
                return Unauthorized("无效身份");

            var seller = await _db.Sellers.FirstOrDefaultAsync(s => s.Id == sellerId);
            if (seller == null)
                return NotFound("商户不存在");

            if (string.IsNullOrEmpty(seller.OpenId))
                return BadRequest("请先绑定微信");

            var nonceStr = Guid.NewGuid().ToString("N");
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var outTradeNo = "SUB" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(1000, 9999);

            var body = new
            {
                appid = AppId,
                mchid = MchId,
                description = "Synerixis AI 月度订阅",
                out_trade_no = outTradeNo,
                notify_url = NotifyUrl,
                amount = new { total = 9900, currency = "CNY" },
                payer = new { openid = seller.OpenId }
            };

            var jsonBody = JsonSerializer.Serialize(body);

            var signStr = $"POST\n/v3/pay/transactions/jsapi\n{timestamp}\n{nonceStr}\n{jsonBody}\n";

            string signature;
            string serialNo;
            try
            {
                if (!System.IO.File.Exists(CertPath.ToString()))
                {
                    return StatusCode(500, "证书文件不存在：" + CertPath);
                }

                var cert = new X509Certificate2(CertPath, CertPassword, X509KeyStorageFlags.Exportable);
                serialNo = cert.GetSerialNumberString().ToUpperInvariant();

                using var rsa = cert.GetRSAPrivateKey() ?? throw new Exception("无法获取私钥");
                var signBytes = rsa.SignData(Encoding.UTF8.GetBytes(signStr), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                signature = Convert.ToBase64String(signBytes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "证书加载或签名失败：" + ex.Message);
            }

            var authHeader = $"WECHATPAY2-SHA256-RSA2048 mchid=\"{MchId}\",nonce_str=\"{nonceStr}\",timestamp=\"{timestamp}\",serial_no=\"{serialNo}\",signature=\"{signature}\"";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            // 强制添加必须头
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Synerixis-Pay/1.0 (Compatible; .NET)");


            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://api.mch.weixin.qq.com/v3/pay/transactions/jsapi", content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest($"统一下单失败: {response.StatusCode} - {responseContent}");
            }

            var result = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
            var prepayId = result["prepay_id"].ToString();

            // 保存订单
            var order = new PayOrder
            {
                Id = Guid.NewGuid(),
                SellerId = sellerId,
                OutTradeNo = outTradeNo,
                Amount = 99m,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };
            _db.PayOrders.Add(order);
            await _db.SaveChangesAsync();

            // 生成前端 JSAPI 参数（需前端用 appId、timeStamp、nonceStr、package、signType、paySign 调起支付）
            var payParams = new Dictionary<string, object>
            {
                ["appId"] = AppId,
                ["timeStamp"] = timestamp,
                ["nonceStr"] = nonceStr,
                ["package"] = $"prepay_id={prepayId}",
                ["signType"] = "RSA",
                ["paySign"] = signature  // 这里是服务器签名，实际前端需重新签名（见下面说明）
            };

            return Ok(payParams);
        }

        [HttpPost("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> PayCallback()
        {
            try
            {
                var body = await new StreamReader(Request.Body).ReadToEndAsync();
                var notify = JsonSerializer.Deserialize<Dictionary<string, object>>(body);

                var resource = JsonSerializer.Deserialize<Dictionary<string, object>>(notify["resource"].ToString());
                var ciphertext = resource["ciphertext"].ToString();
                var nonce = resource["nonce"].ToString();
                var associatedData = resource["associated_data"]?.ToString() ?? string.Empty;

                var plaintext = AesGcmDecrypt(ciphertext, ApiV3Key, nonce, associatedData);

                var decrypted = JsonSerializer.Deserialize<Dictionary<string, object>>(plaintext);
                var outTradeNo = decrypted["out_trade_no"].ToString();
                var transactionId = decrypted["transaction_id"].ToString();

                var order = await _db.PayOrders.FirstOrDefaultAsync(o => o.OutTradeNo == outTradeNo);
                if (order == null || order.Status == "paid")
                    return Content("{\"code\":\"SUCCESS\",\"message\":\"OK\"}");

                order.Status = "paid";
                order.TransactionId = transactionId;
                order.PaidAt = DateTime.UtcNow;

                var seller = await _db.Sellers.FirstOrDefaultAsync(s => s.Id == order.SellerId);
                if (seller != null)
                {
                    seller.ApplySubscription(99);  // 使用封装方法
                }

                await _db.SaveChangesAsync();

                return Content("{\"code\":\"SUCCESS\",\"message\":\"OK\"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("回调异常: " + ex.Message);
                return Content("{\"code\":\"FAIL\",\"message\":\"服务器错误\"}");
            }
        }

        private string AesGcmDecrypt(string ciphertext, string key, string nonce, string associatedData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32, '\0'));
            var nonceBytes = Encoding.UTF8.GetBytes(nonce);
            var associatedDataBytes = Encoding.UTF8.GetBytes(associatedData);
            var cipherBytes = Convert.FromBase64String(ciphertext);

            using var aesGcm = new AesGcm(keyBytes);
            var plaintextBytes = new byte[cipherBytes.Length - AesGcm.TagByteSizes.MaxSize];

            aesGcm.Decrypt(nonceBytes, cipherBytes.AsSpan(0, plaintextBytes.Length), cipherBytes.AsSpan(plaintextBytes.Length), plaintextBytes, associatedDataBytes);

            return Encoding.UTF8.GetString(plaintextBytes);
        }
    }
}