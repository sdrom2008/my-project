using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Synerixis.Api.Controllers;
using Synerixis.Application.DTOs;
using Synerixis.Application.Interfaces;
using Synerixis.Domain.Common;
using Synerixis.Domain.Entities;
using Synerixis.Infrastructure.Data;
using Synerixis.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly HttpClient _http;
    private readonly IAuthService _authService;
    private readonly AliyunSmsService _smsService;
    private readonly IMemoryCache _cache;  // 用于缓存验证码等

    public AuthController(AppDbContext db, IAuthService authService, IConfiguration config, IHttpClientFactory factory, AliyunSmsService smsService,IMemoryCache cache)
    {
        _db = db;
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _config = config;
        _http = factory.CreateClient();
        _smsService = smsService;
        _cache = cache;
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("wechat")]
    public async Task<IActionResult> WeChatLogin([FromBody] WeChatCode request)
    {
        if (string.IsNullOrEmpty(request.Code)) return BadRequest("Code required");

        var appId = _config["WeChat:AppId"];
        var secret = _config["WeChat:AppSecret"];
        var url = $"https://api.weixin.qq.com/sns/jscode2session?appid={appId}&secret={secret}&js_code={request.Code}&grant_type=authorization_code";

        var resp = await _http.GetFromJsonAsync<WeChatResp>(url);
        if (resp?.OpenId == null) return BadRequest(resp?.Errmsg ?? "微信登录失败");

        var openId = resp.OpenId;

        var seller = await _db.Sellers.FirstOrDefaultAsync(s => s.OpenId == openId);

        if (seller == null)
        {
            // 未绑定，返回 openid，让前端引导绑定手机号
            return Ok(new { needBind = true, openid = openId });
        }
        // 已绑定，直接登录
        seller.RecordLogin("wechat");
        await _db.SaveChangesAsync();

        var token = _authService.GenerateJwt(seller.Id);
        Console.WriteLine("生成 JWT 时使用的 sellerId = " + seller.Id.ToString());
        return Ok(new
        {
            token,
            sellerId = seller.Id,
            nickname = seller.Nickname,
            avatarUrl = seller.AvatarUrl,
            subscriptionLevel = seller.SubscriptionLevel,
            freeQuota = seller.FreeQuota
        });
    }

    /// <summary>
    /// 手机登录
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("phone-login")]
    public async Task<IActionResult> PhoneLogin([FromBody] PhoneLoginDto dto)
    {
        if (string.IsNullOrEmpty(dto.Phone) || string.IsNullOrEmpty(dto.Code))
            return BadRequest("手机号和验证码不能为空");

        // 校验验证码（这里假设你有验证码服务，示例用缓存模拟）
        if (!_cache.TryGetValue($"sms:{dto.Phone}", out string cachedCode) || cachedCode != dto.Code)
            return BadRequest("验证码错误或已过期");

        var seller = await _db.Sellers.FirstOrDefaultAsync(s => s.Phone == dto.Phone);

        bool isNew = false;
        if (seller == null)
        {
            isNew = true;
            // 自动注册新商户
            seller = Seller.CreateWithPhone(dto.Phone);
            _db.Sellers.Add(seller);
        }

        seller.RecordLogin("phone");
        await _db.SaveChangesAsync();

        var token = _authService.GenerateJwt(seller.Id);
        return Ok(new
        {
            token,
            sellerId = seller.Id,
            nickname = seller.Nickname,
            freeQuota = seller.FreeQuota,
            subscriptionLevel = seller.SubscriptionLevel,
            isNewRegistration = isNew
        });
    }

    /// <summary>
    /// 绑定微信
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("bind-wechat")]
    public async Task<IActionResult> BindWechat([FromBody] BindWechatDto dto)
    {
        if (string.IsNullOrEmpty(dto.OpenId) || string.IsNullOrEmpty(dto.Phone))
            return BadRequest("参数缺失");

        var seller = await _db.Sellers.FirstOrDefaultAsync(s => s.Phone == dto.Phone);

        if (seller == null)
            return BadRequest("手机号未注册，请先注册");

        // 检查 openid 是否已被其他商户绑定
        var existing = await _db.Sellers.FirstOrDefaultAsync(s => s.OpenId == dto.OpenId);
        if (existing != null && !(existing.Id ==seller.Id))
            return BadRequest("该微信已绑定其他账号");

        seller.BindWechat(dto.OpenId);
        seller.RecordLogin("wechat");
        await _db.SaveChangesAsync();

        var token = _authService.GenerateJwt(seller.Id);
        return Ok(new
        {
            token,
            sellerId = seller.Id,
            nickname = seller.Nickname,
            freeQuota = seller.FreeQuota
        });
    }

    /// <summary>
    /// 发送验证码
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("send-code")]
    public async Task<IActionResult> SendCode([FromBody] SendCodeDto dto)
    {
        if (string.IsNullOrEmpty(dto.Phone) || dto.Phone.Length != 11 || !dto.Phone.StartsWith("1"))
            return BadRequest("手机号格式错误");

        var code = new Random().Next(100000, 999999).ToString();

        var success = await _smsService.SendVerificationCodeAsync(dto.Phone, code);

        if (!success)
            return StatusCode(500, "发送验证码失败，请稍后重试");

        // 存到缓存，5分钟过期
        _cache.Set($"sms:{dto.Phone}", code, TimeSpan.FromMinutes(5));

        return Ok(new { message = "验证码已发送，5分钟内有效" });
    }

    /// <summary>
    /// 绑定电话
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("bind-phone")]
    public async Task<IActionResult> BindPhone([FromBody] BindPhoneDto dto)
    {
        // dto.openid + dto.phone + dto.code

        // 校验验证码...

        var seller = await _db.Sellers.FirstOrDefaultAsync(s => s.Phone == dto.Phone);

        if (seller == null)
        {
            // 新注册
            seller = Seller.CreateWithPhone(dto.Phone);
            seller.BindWechat(dto.OpenId);
            _db.Sellers.Add(seller);
        }
        else
        {
            // 已有手机号，绑定微信
            seller.BindWechat(dto.OpenId);
        }

        seller.RecordLogin("phone");
        await _db.SaveChangesAsync();

        var token = _authService.GenerateJwt(seller.Id);
        return Ok(new { token, sellerId = seller.Id });
    }

    [HttpPost("decrypt-phone")]
    public async Task<IActionResult> DecryptPhone([FromBody] DecryptPhoneDto dto)
    {
        // dto: code (wx.login 的 code), encryptedData, iv

        // 1. 用 code 换 session_key 和 openid
        var appId = _config["WeChat:AppId"];
        var secret = _config["WeChat:AppSecret"];
        var url = $"https://api.weixin.qq.com/sns/jscode2session?appid={appId}&secret={secret}&js_code={dto.Code}&grant_type=authorization_code";

        var resp = await _http.GetFromJsonAsync<WeChatSessionResp>(url);
        if (resp?.OpenId == null || string.IsNullOrEmpty(resp.SessionKey))
            return BadRequest("微信授权失败");

        var openId = resp.OpenId;
        var sessionKey = resp.SessionKey;



        // 2. 解密手机号（用微信官方算法）
        try
        {
            var phoneInfo = WxDecryptHelper.DecryptPhone(dto.EncryptedData, dto.Iv, sessionKey, appId);
            var phone = phoneInfo.PhoneNumber;

            // 3. 查找或创建商户
            var seller = await _db.Sellers.FirstOrDefaultAsync(s => s.OpenId == openId);

            if (seller == null)
            {
                // 新商户，用手机号注册
                seller = Seller.CreateWithPhone(phone);
                seller.BindWechat(openId);
                _db.Sellers.Add(seller);
            }
            else
            {
                // 已存在，绑定手机号（如果没绑）
                if (string.IsNullOrEmpty(seller.Phone))
                {
                    seller.BindPhone(phone);
                }
            }

            seller.RecordLogin("wechat");
            await _db.SaveChangesAsync();

            var token = _authService.GenerateJwt(seller.Id);
            return Ok(new
            {
                token,
                sellerId = seller.Id,
                nickname = seller.Nickname,
                freeQuota = seller.FreeQuota
            });
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "解密手机号失败");
            return BadRequest("授权失败，请重试");
        }
    }
}
public class SendCodeDto
{
    public string Phone { get; set; }
}

public class BindPhoneDto
{
    public string OpenId { get; set; }
    public string Phone { get; set; }
    public string Code { get; set; }
}

public class WeChatCode { public string Code { get; set; } = null!; }
public class WeChatResp
{
    public string OpenId { get; set; } = null!;
    public string? Errmsg { get; set; }
}