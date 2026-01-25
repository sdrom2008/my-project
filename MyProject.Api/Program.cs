using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyProject.Application.Interfaces;
using MyProject.Infrastructure.Agents;
using MyProject.Infrastructure.AI;
using MyProject.Infrastructure.Data;
using MyProject.Infrastructure.Repositories;
using MyProject.Infrastructure.Services;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();  // ← 必须加这一行！注册所有 Controller

// Add services to the container.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAiChatService, AiChatService>();

// 注册意图分类器
builder.Services.AddScoped<IIntentClassifier, KeywordIntentClassifier>();
// 注册 AgentRouter
builder.Services.AddScoped<AgentRouter>();
// 注册具体 Agent
builder.Services.AddScoped<IAgent, ProductOptimizationAgent>();
// 未来其他 Agent 类似 AddScoped<IAgent, XxxAgent>();

//ai 单例配置
builder.Services.AddSingleton<SemanticKernelConfig>();
// 泛型仓储（推荐）
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
// 如果用了专用仓储
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();

builder.Services.AddHttpClient();  // HttpClient 工厂

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
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

// EF Core + MySQL
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var conn = builder.Configuration.GetConnectionString("MySqlConnection");
    opt.UseMySql(conn, ServerVersion.AutoDetect(conn), mysql => mysql.EnableRetryOnFailure());
});

// 如果用 Health Checks，添加
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("Database");

var app = builder.Build();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
