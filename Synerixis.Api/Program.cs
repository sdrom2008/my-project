using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
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

// 添加控制器支持
builder.Services.AddControllers();

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


// Semantic Kernel 配置
builder.Services.AddSingleton<SemanticKernelConfig>();

// 泛型仓储（推荐只注册一次）
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// 专用仓储（如果有）
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();

// 业务服务
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAiChatService, AiChatService>();
builder.Services.AddScoped<IGeneralChatAgent, GeneralChatAgent>();

// 意图分类器（用 LLM 版本）
builder.Services.AddScoped<IIntentClassifier, IntentClassifier>();

// AgentRouter（必须在所有 Agent 注册后）
builder.Services.AddScoped<AgentRouter>();

// 具体 Agent（多实现，AgentRouter 会用它们）
builder.Services.AddScoped<IAgent, ProductOptimizationAgent>();
// IChatCompletionService（如果 Agent 或 Classifier 直接依赖它）
builder.Services.AddScoped<IChatCompletionService>(sp =>
    sp.GetRequiredService<SemanticKernelConfig>().Kernel.GetRequiredService<IChatCompletionService>());


// HttpClient 工厂（如果 Agent 里需要调用外部 API）
builder.Services.AddHttpClient();

// Health Checks（可选）
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("Database");

var app = builder.Build();

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