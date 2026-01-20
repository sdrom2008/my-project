using Microsoft.EntityFrameworkCore;
using MyProject.Application.Services;
using MyProject.Infrastructure.AIServices;
using MyProject.Infrastructure.Data;
using MyProject.Infrastructure.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IKernelService, KernelService>();

builder.Services.AddScoped<IAiChatService, MyProject.Application.Services.AiChatService>();

// 添加 EF Core + MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=localhost;Database=myproject_db;User=root;Password=你的密码;";

    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure());
});

// 如果用 Health Checks，添加
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("Database");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
