using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyProject.Application.Interfaces;
using MyProject.Domain.Entities;
using MyProject.Infrastructure.Data;
using MyProject.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
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
            seller = Seller.Create(openId);
            _db.Sellers.Add(seller);
            await _db.SaveChangesAsync();
            Console.WriteLine("新 Seller 保存成功，Id = " + seller.Id.ToString());
        }
        else
        {
            Console.WriteLine("找到已有 Seller，Id = " + seller.Id.ToString());
        }

        seller.RecordLogin();  // 更新登录时间
        await _db.SaveChangesAsync();

        var token = _authService.GenerateJwt(seller.Id);
        Console.WriteLine("生成 JWT 时使用的 sellerId = " + seller.Id.ToString());
        return Ok(new
        {
            token,
            sellerId = seller.Id.ToString(),
            nickname = seller.Nickname,
            avatarUrl = seller.AvatarUrl,
            subscriptionLevel = seller.SubscriptionLevel
        });
    } 
}

public class WeChatCode { public string Code { get; set; } = null!; }
public class WeChatResp
{
    public string OpenId { get; set; } = null!;
    public string? Errmsg { get; set; }
}