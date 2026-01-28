using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Synerixis.Infrastructure.Services;
using Synerixis.Api.Controllers;
using Synerixis.Application.Interfaces;
using Synerixis.Domain.Entities;
using Synerixis.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Synerixis.Application.DTOs;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly HttpClient _http;
    private readonly IAuthService _authService;

    public AuthController(AppDbContext db, IAuthService authService, IConfiguration config, IHttpClientFactory factory)
    {
        _db = db;
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _config = config;
        _http = factory.CreateClient();
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
        //var cachedCode = await _cache.GetAsync<string>($"sms:{dto.Phone}");
        //if (cachedCode != dto.Code) return BadRequest("验证码错误");

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
        if (string.IsNullOrEmpty(dto.Phone) || dto.Phone.Length != 11)
            return BadRequest("手机号格式错误");

        // 生成 4 位验证码
        var code = "6666";  //new Random().Next(1000, 9999).ToString();

        // 实际发短信（这里模拟，替换成你的短信 SDK）
        // await _smsService.SendAsync(dto.Phone, $"您的验证码是 {code}，5分钟有效");

        // 存到缓存（5 分钟过期）
        //await _cache.SetAsync($"sms:{dto.Phone}", code, TimeSpan.FromMinutes(5));

        return Ok(new { message = "验证码已发送" });
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