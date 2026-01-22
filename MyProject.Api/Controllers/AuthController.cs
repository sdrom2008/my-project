using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyProject.Domain.Entities;
using MyProject.Infrastructure.Data;
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

    public AuthController(AppDbContext db, IConfiguration config, IHttpClientFactory factory)
    {
        _db = db;
        _config = config;
        _http = factory.CreateClient();
    }

    [HttpPost("wechat")]
    public async Task<IActionResult> WeChatLogin([FromBody] WeChatCode code)
    {
        if (string.IsNullOrEmpty(code.Code)) return BadRequest("Code required");

        var appId = _config["WeChat:AppId"];
        var secret = _config["WeChat:AppSecret"];
        var url = $"https://api.weixin.qq.com/sns/jscode2session?appid={appId}&secret={secret}&js_code={code.Code}&grant_type=authorization_code";

        var resp = await _http.GetFromJsonAsync<WeChatResp>(url);
        if (resp?.OpenId == null) return BadRequest(resp?.Errmsg ?? "微信登录失败");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.OpenId == resp.OpenId);
        if (user == null)
        {
            user = MyProject.Domain.Entities.User.Create(resp.OpenId);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        var token = GenerateJwt(user.Id);
        return Ok(new { token, userId = user.Id.ToString(), nickname = user.Nickname });
    }

    private string GenerateJwt(Guid userId)
    {
        var claims = new[] { new Claim("userId", userId.ToString()) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"]!)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class WeChatCode { public string Code { get; set; } = null!; }
public class WeChatResp
{
    public string OpenId { get; set; } = null!;
    public string? Errmsg { get; set; }
}