using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Senparc.Weixin.RegisterServices;
using Synerixis.Application.Agents;
using Synerixis.Application.Interfaces;
using Synerixis.Application.Services;
using Synerixis.Domain.Entities;
using Synerixis.Infrastructure.Agent;
using Synerixis.Infrastructure.AI;
using Synerixis.Infrastructure.Data;
using Synerixis.Infrastructure.Repositories;
using Synerixis.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();          // 输出到控制台
builder.Logging.AddDebug();            // 输出到调试窗口（如 VS Output）
builder.Logging.SetMinimumLevel(LogLevel.Debug);  // 必须设为 Debug 才能看到 LogDebug

// 1 添加控制器支持
builder.Services.AddControllers();


// 2 Semantic Kernel 配置
builder.Services.AddSingleton<SemanticKernelConfig>();

builder.Services.AddSingleton<Kernel>(sp => sp.GetRequiredService<SemanticKernelConfig>().Kernel);

builder.Services.AddScoped<IChatCompletionService>(sp =>
    sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());

// 4. 业务服务（顺序：先基础，后依赖）
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGeneralChatAgent, GeneralChatAgent>();
builder.Services.AddScoped<IIntentClassifier, IntentClassifier>();
// 5. Agent 先注册（所有具体 Agent）
builder.Services.AddScoped<IAgent, ProductOptimizationAgent>();
// 如果有其他 Agent，在这里继续加
builder.Services.AddSingleton<AliyunSmsService>();

builder.Services.AddScoped<ProductService>();

// 内存缓存（用于意图分类等）
builder.Services.AddMemoryCache();

// 专用仓储（如果有）
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();


// 意图分类器（用 LLM 版本）
builder.Services.AddScoped<IIntentClassifier, IntentClassifier>();


// 6. AgentRouter（注册为接口！必须在所有 Agent 后）
builder.Services.AddScoped<IAgentRouter, AgentRouter>();

#region 添加微信配置
builder.Services.AddSenparcWeixinServices(builder.Configuration);

#endregion

// 添加 OpenAPI/Swagger（可选，开发时方便）
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// CORS（允许所有，生产环境建议收紧）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// JWT 认证
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// EF Core + MySQL
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var conn = builder.Configuration.GetConnectionString("MySqlConnection");
    opt.UseMySql(conn, ServerVersion.AutoDetect(conn), mysql => mysql.EnableRetryOnFailure());
});

// 泛型仓储（推荐只注册一次）
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// HttpClient 工厂（如果 Agent 里需要调用外部 API）
builder.Services.AddHttpClient();

// Health Checks（可选）
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("Database");

// 7. AiChatService（最后注册，依赖 Router）
builder.Services.AddScoped<IAiChatService, AiChatService>();

var app = builder.Build();

//配置静态文件服务
app.UseStaticFiles();

// 中间件管道
app.UseCors("AllowAll");

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();